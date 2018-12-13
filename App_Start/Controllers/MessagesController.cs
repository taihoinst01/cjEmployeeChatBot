using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using cjEmployeeChatBot.DB;
using cjEmployeeChatBot.Models;
using Newtonsoft.Json.Linq;

using System.Configuration;
using System.Web.Configuration;
using cjEmployeeChatBot.Dialogs;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.ConnectorEx;
using cjEmployeeChatBot.SAP;
using SSODecodeCJW;
using System.Threading;

namespace cjEmployeeChatBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        //MessagesController
        public static readonly string TEXTDLG = "2";
        public static readonly string CARDDLG = "3";
        public static readonly string MEDIADLG = "4";

        public static Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration("/");
        const string chatBotAppID = "appID";
        public static int appID = Convert.ToInt32(rootWebConfig.ConnectionStrings.ConnectionStrings[chatBotAppID].ToString());

        //config 변수 선언
        static public string[] LUIS_NM = new string[5];        //루이스 이름
        static public string[] LUIS_APP_ID = new string[5];    //루이스 app_id
        static public string LUIS_SUBSCRIPTION = "";            //루이스 구독키
        static public int LUIS_TIME_LIMIT;                      //루이스 타임 체크
        static public string BOT_ID = "";                       //bot id
        static public string MicrosoftAppId = "";               //app id
        static public string MicrosoftAppPassword = "";         //app password
        static public string LUIS_SCORE_LIMIT = "";             //루이스 점수 체크

        //public static int sorryMessageCnt = 0;
        //public static int suggetionsMessageCnt = 0;
        public static int chatBotID = 0;

        public static List<RelationList> relationList = new List<RelationList>();
        public static string luisId = "";
        public static string luisIntent = "";
        public static string luisEntities = "";
        public static string luisIntentScore = "";
        public static string luistTpyeEntities = "";
        public static string dlgId = "";        
        public static string queryStr = "";
        public static string luisQuery = "";        
        public static DateTime startTime;

        //사용자ID
        public static string userID = "";

        //건의사항
        public static string suggestions = "";

        public static CacheList cacheList = new CacheList();
        //결과 플레그 H : 정상 답변,  G : 건의사항, D : 답변 실패, E : 에러, S : SMALLTALK
        public static String replyresult = "";
        public static String apiFlag = "";

        public static string channelID = "";

        public static DbConnect db = new DbConnect();
        public static DButil dbutil = new DButil();
        public static TestEaiCall tec = new TestEaiCall();

        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {

            //DbConnect db = new DbConnect();
            //DButil dbutil = new DButil();
            DButil.HistoryLog("db connect !! ");
            //HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            HttpResponseMessage response;

            DButil.HistoryLog("activity.CreateReply() !! ");
            Activity reply1 = activity.CreateReply();
            Activity reply2 = activity.CreateReply();
            Activity reply3 = activity.CreateReply();
            Activity reply4 = activity.CreateReply();

            DButil.HistoryLog("SetActivity!! ");
            // Activity 값 유무 확인하는 익명 메소드
            Action<Activity> SetActivity = (act) =>
            {
                if (!(reply1.Attachments.Count != 0 || reply1.Text != ""))
                {
                    reply1 = act;
                }
                else if (!(reply2.Attachments.Count != 0 || reply2.Text != ""))
                {
                    reply2 = act;
                }
                else if (!(reply3.Attachments.Count != 0 || reply3.Text != ""))
                {
                    reply3 = act;
                }
                else if (!(reply4.Attachments.Count != 0 || reply4.Text != ""))
                {
                    reply4 = act;
                }
                else
                {

                }
            };

            
            //node통하여 dll 호출... 제발 되라...
            using (HttpClient client = new HttpClient())
            {
                //취소 시간 설정
                string url = "https://cjemployeeconnect.azurewebsites.net/";
                client.Timeout = TimeSpan.FromMilliseconds(5000); //5초
                var cts = new CancellationTokenSource();
                try
                {
                    HttpResponseMessage msg = await client.GetAsync(url, cts.Token);
                    var request_msg = await msg.Content.ReadAsStringAsync();
                    Debug.WriteLine("msg=====" + request_msg);
                    DButil.HistoryLog("msg=====" + request_msg);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ex.Message=====" + ex.Message);
                    DButil.HistoryLog("ex.Message=====" + ex.Message);
                }
            }            
            //사용자 계정 처리
            //if (activity.Contains("userid"))
            //{
            //    db.UserDataInsert(activity.ChannelId, activity.Conversation.Id);
            //    //첫번쨰 메세지 출력 x
            //    response = Request.CreateResponse(HttpStatusCode.OK);
            //    return response;
            //}

            //건의사항용 userData 선언
            List<UserData> userData = db.UserDataConfirm(activity.ChannelId, activity.Conversation.Id);

            if (userData.Count() == 0)
            {
                db.UserDataInsert(activity.ChannelId, activity.Conversation.Id);
            } 

            if (activity.Type == ActivityTypes.ConversationUpdate && activity.MembersAdded.Any(m => m.Id == activity.Recipient.Id))
            {
                startTime = DateTime.Now;

                //DButil.HistoryLog("activity.Text111111=====" + activity.Text);

                //파라메터 호출
                if (LUIS_NM.Count(s => s != null) > 0)
                {
                    //string[] LUIS_NM = new string[10];
                    Array.Clear(LUIS_NM, 0, LUIS_NM.Length);
                }

                if (LUIS_APP_ID.Count(s => s != null) > 0)
                {
                    //string[] LUIS_APP_ID = new string[10];
                    Array.Clear(LUIS_APP_ID, 0, LUIS_APP_ID.Length);
                }
                //Array.Clear(LUIS_APP_ID, 0, 10);
                DButil.HistoryLog("db SelectConfig start !! ");
                List<ConfList> confList = db.SelectConfig();
                DButil.HistoryLog("db SelectConfig end!! ");

                for (int i = 0; i < confList.Count; i++)
                {
                    switch (confList[i].cnfType)
                    {
                        case "LUIS_APP_ID":
                            LUIS_APP_ID[LUIS_APP_ID.Count(s => s != null)] = confList[i].cnfValue;
                            LUIS_NM[LUIS_NM.Count(s => s != null)] = confList[i].cnfNm;
                            break;
                        case "LUIS_SUBSCRIPTION":
                            LUIS_SUBSCRIPTION = confList[i].cnfValue;
                            break;
                        case "BOT_ID":
                            BOT_ID = confList[i].cnfValue;
                            break;
                        case "MicrosoftAppId":
                            MicrosoftAppId = confList[i].cnfValue;
                            break;
                        case "MicrosoftAppPassword":
                            MicrosoftAppPassword = confList[i].cnfValue;
                            break;
                        case "LUIS_SCORE_LIMIT":
                            LUIS_SCORE_LIMIT = confList[i].cnfValue;
                            break;
                        case "LUIS_TIME_LIMIT":
                            LUIS_TIME_LIMIT = Convert.ToInt32(confList[i].cnfValue);
                            break;
                        default: //미 정의 레코드
                            Debug.WriteLine("*conf type : " + confList[i].cnfType + "* conf value : " + confList[i].cnfValue);
                            DButil.HistoryLog("*conf type : " + confList[i].cnfType + "* conf value : " + confList[i].cnfValue);
                            break;
                    }
                }

                Debug.WriteLine("* DB conn : " + activity.Type);
                DButil.HistoryLog("* DB conn : " + activity.Type);

                //초기 다이얼로그 호출
                List<DialogList> dlg = db.SelectInitDialog(activity.ChannelId);

                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                foreach (DialogList dialogs in dlg)
                {
                    Activity initReply = activity.CreateReply();
                    initReply.Recipient = activity.From;
                    initReply.Type = "message";
                    initReply.Attachments = new List<Attachment>();
                    //initReply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                    Attachment tempAttachment;

                    if (dialogs.dlgType.Equals(CARDDLG))
                    {
                        foreach (CardList tempcard in dialogs.dialogCard)
                        {
                            tempAttachment = dbutil.getAttachmentFromDialog(tempcard, activity);
                            initReply.Attachments.Add(tempAttachment);

                            //2018-11-26:KSO:INIT Carousel 만드는부분 추가
                            if (tempcard.card_order_no > 1)
                            {
                                initReply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            }
                        }
                    }
                    else
                    {
                        tempAttachment = dbutil.getAttachmentFromDialog(dialogs, activity);
                        initReply.Attachments.Add(tempAttachment);
                    }
                    await connector.Conversations.SendToConversationAsync(initReply);
                }

                //현재위치사용승인 테스트
                //Activity replyLocation = activity.CreateReply();
                //replyLocation.Recipient = activity.From;
                //replyLocation.Type = "message";
                //replyLocation.Attachments = new List<Attachment>();
                //replyLocation.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                //replyLocation.Attachments.Add(
                //    GetHeroCard_facebookMore(
                //    "", "",
                //    "현재 위치 사용 승인",
                //    new CardAction(ActionTypes.ImBack, "현재 위치 사용 승인", value: MessagesController.queryStr))
                //);
                //await connector.Conversations.SendToConversationAsync(replyLocation);

                //Debug.WriteLine("testEaiCall.ToString()====" + testEaiCall.call);

                DateTime endTime = DateTime.Now;
                Debug.WriteLine("프로그램 수행시간 : {0}/ms", ((endTime - startTime).Milliseconds));
                Debug.WriteLine("* activity.Type : " + activity.Type);
                Debug.WriteLine("* activity.Recipient.Id : " + activity.Recipient.Id);
                Debug.WriteLine("* activity.ServiceUrl : " + activity.ServiceUrl);

                DButil.HistoryLog("* activity.Type : " + activity.ChannelData);
                DButil.HistoryLog("* activity.Recipient.Id : " + activity.Recipient.Id);
                DButil.HistoryLog("* activity.ServiceUrl : " + activity.ServiceUrl);
            }
            else if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                try
                {                    
                    Debug.WriteLine("* activity.Type == ActivityTypes.Message ");
                    channelID = activity.ChannelId;
                    string orgMent = activity.Text;
                    DButil.HistoryLog("* activity.Text : " + activity.Text); 
                    //현재위치사용승인
                    if (orgMent.Contains("current location") || orgMent.Equals("현재위치사용승인"))
                    {
                        if (!orgMent.Contains(':'))
                        {
                            //첫번쨰 메세지 출력 x
                            response = Request.CreateResponse(HttpStatusCode.OK);
                            return response;
                        }
                        else
                        {
                            //위도경도에 따른 값 출력
                            try
                            {
                                string location = orgMent.Replace("current location:", "");
                                //테스트용
                                //string location = "129.0929788:35.2686635";
                                string[] location_result = location.Split(':');
                                //regionStr = db.LocationValue(location_result[1], location_result[2]);
                                DButil.HistoryLog("*regionStr : " + location_result[0] + " " + location_result[1]);
                                Debug.WriteLine("*regionStr : " + location_result[0] + " " + location_result[1]);
                                DButil.mapSave(location_result[0], location_result[1]);
                                Activity reply_brach = activity.CreateReply();
                                reply_brach.Recipient = activity.From;
                                reply_brach.Type = "message";
                                reply_brach.Attachments = new List<Attachment>();
                                reply_brach.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                reply_brach.Attachments.Add(
                                    DButil.GetHeroCard_Map(
                                    "타이호인스트",
                                    "연락처",
                                    "주소",
                                    new CardImage(url: "https://cjEmployeeChatBot.azurewebsites.net/image/map/"+ location_result[1] + ","+ location_result[0] + ".png"),
                                    new CardAction(ActionTypes.OpenUrl, "타이호인스트", value: "http://www.taihoinst.com/"),
                                    location_result[1],
                                    location_result[0])
                                    );
                                var reply_brach1 = await connector.Conversations.SendToConversationAsync(reply_brach);
                                response = Request.CreateResponse(HttpStatusCode.OK);
                                return response;
                            }
                            catch
                            {
                                queryStr = "서울 시승센터";
                            }
                        }
                    }

                    //apiFlag = "COMMON";

                    //대화 시작 시간
                    startTime = DateTime.Now;
                    long unixTime = ((DateTimeOffset)startTime).ToUnixTimeSeconds();

                    DButil.HistoryLog("orgMent : " + orgMent);
                    //금칙어 체크
                    CardList bannedMsg = db.BannedChk(orgMent);
                    Debug.WriteLine("* bannedMsg : " + bannedMsg.cardText);//해당금칙어에 대한 답변

                    if (bannedMsg.cardText != null)
                    {
                        Activity reply_ment = activity.CreateReply();
                        reply_ment.Recipient = activity.From;
                        reply_ment.Type = "message";
                        reply_ment.Text = bannedMsg.cardText;

                        var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);
                        response = Request.CreateResponse(HttpStatusCode.OK);
                        return response;
                    }
                    else
                    {
                        //queryStr = orgMent;
                        ////인텐트 엔티티 검출
                        ////캐시 체크
                        //cashOrgMent = Regex.Replace(orgMent, @"[^a-zA-Z0-9ㄱ-힣]", "", RegexOptions.Singleline);
                        //cacheList = db.CacheChk(cashOrgMent.Replace(" ", ""));                     // 캐시 체크 (TBL_QUERY_ANALYSIS_RESULT 조회..)

                        //정규화
                        luisQuery = orgMent;
                        orgMent = Regex.Replace(orgMent, @"[^a-zA-Z0-9ㄱ-힣]", "", RegexOptions.Singleline);
                        orgMent = orgMent.Replace(" ", "").ToLower();
                        queryStr = orgMent;
                        //cacheList = db.CacheChk(cashOrgMent.Replace(" ", ""));                     // 캐시 체크 (TBL_QUERY_ANALYSIS_RESULT 조회..)
                        //cacheList.luisIntent 초기화
                        cacheList.luisIntent = null;

                        
                        

                        //건의사항
                        if (orgMent.Contains("건의사항")|| orgMent.Contains("건의 사항"))
                        {
                            if (userData[0].conversationsId == activity.Conversation.Id)
                            {
                                //suggestions = "Y";
                                db.UserDataUpdate(activity.ChannelId, activity.Conversation.Id, 1);
                                userData[0].loop = 1;
                            }
                        }
                        //smalltalk 문자 확인                        
                        String smallTalkSentenceConfirm = db.SmallTalkSentenceConfirm;

                        //SAP 용어 확인
                        string qnAMakerAnswer = dbutil.GetQnAMaker(luisQuery);

                        //smalltalk 답변이 있을경우
                        if (!string.IsNullOrEmpty(smallTalkSentenceConfirm))
                        {
                            luisId = "";
                        }
                        else if  (qnAMakerAnswer.Equals("No good match found in KB"))
                        {
                            luisId = "";                            
                        }
                        //luis 호출
                        else if (cacheList.luisIntent == null || cacheList.luisEntities == null)
                        {
                            DButil.HistoryLog("cache none : " + orgMent);
                            Debug.WriteLine("cache none : " + orgMent);
                            //루이스 체크(intent를 루이스를 통해서 가져옴)
                            //cacheList.luisId = dbutil.GetMultiLUIS(orgMent);
                            //Debug.WriteLine("cacheList.luisId : " + cacheList.luisId);

                            cacheList.luisIntent = dbutil.GetMultiLUIS(luisQuery);
                            Debug.WriteLine("cacheList.luisIntent : " + cacheList.luisIntent);
                            //Debug.WriteLine("cacheList.luisEntitiesValue : " + cacheList.luisEntitiesValue);
                            cacheList = db.CacheDataFromIntent(cacheList.luisIntent);

                            luisId = cacheList.luisId;
                            luisIntent = cacheList.luisIntent;
                            luisEntities = cacheList.luisEntities;
                            luisIntentScore = cacheList.luisScore;

                        }

                        DButil.HistoryLog("luisId : " + luisId);
                        DButil.HistoryLog("luisIntent : " + luisIntent);
                        DButil.HistoryLog("luisEntities : " + luisEntities);

                        //SAP 비밀번호 초기화
                        //tec.call();

                        string smallTalkConfirm = "";

                        if (!string.IsNullOrEmpty(luisIntent))
                        {
                            relationList = db.DefineTypeChkSpare(cacheList.luisIntent, cacheList.luisEntities);
                        }
                        else
                        {
                            relationList = null;
                            //smalltalk 답변가져오기
                            if (orgMent.Length < 13)
                            {
                                smallTalkConfirm = db.SmallTalkConfirm;
                            } else
                            {
                                smallTalkConfirm = "";
                            }
                            
                        }

                        //if (apiFlag.Equals("COMMON") && relationList != null)
                        //if (relationList != null && string.IsNullOrEmpty(userData.GetProperty<string>("suggestion")))
                        if (relationList != null)
                        {
                            dlgId = "";
                            for (int m = 0; m < MessagesController.relationList.Count; m++)
                            {
                                DialogList dlg = db.SelectDialog(MessagesController.relationList[m].dlgId);
                                dlgId += Convert.ToString(dlg.dlgId) + ",";
                                Activity commonReply = activity.CreateReply();
                                Attachment tempAttachment = new Attachment();
                                DButil.HistoryLog("dlg.dlgType : " + dlg.dlgType);

                                if (dlg.dlgType.Equals(CARDDLG))
                                {
                                    foreach (CardList tempcard in dlg.dialogCard)
                                    {
                                        
                                        tempAttachment = dbutil.getAttachmentFromDialog(tempcard, activity);

                                        if (tempAttachment != null)
                                        {
                                            commonReply.Attachments.Add(tempAttachment);
                                        }

                                        //2018-04-19:KSO:Carousel 만드는부분 추가
                                        if (tempcard.card_order_no > 1)
                                        {
                                            commonReply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                        }

                                    }
                                }
                                else
                                {
                                    //DButil.HistoryLog("* facebook dlg.dlgId : " + dlg.dlgId);
                                    DButil.HistoryLog("* activity.ChannelId : " + activity.ChannelId);
                                  
                                    tempAttachment = dbutil.getAttachmentFromDialog(dlg, activity);
                                    commonReply.Attachments.Add(tempAttachment);
                                }

                                if (commonReply.Attachments.Count > 0)
                                {
                                    SetActivity(commonReply);
                                    //if (!string.IsNullOrEmpty(userData.GetProperty<string>("suggestion")))
                                    //{
                                    //    replyresult = "G";
                                    //}
                                    //else
                                    //{
                                        replyresult = "H";
                                    //}

                                }
                            }
                        }
                        //SMALLTALK 확인
                        //else if (!string.IsNullOrEmpty(smallTalkConfirm) && string.IsNullOrEmpty(userData.GetProperty<string>("suggestion")))
                        else if (!string.IsNullOrEmpty(smallTalkConfirm))
                        {
                            Debug.WriteLine("smalltalk dialogue-------------");

                            Random rand = new Random();
                            
                            //SMALLTALK 구분
                            string[] smallTalkConfirm_result = smallTalkConfirm.Split('$');

                            int smallTalkConfirmNum = rand.Next(0, smallTalkConfirm_result.Length);

                            Activity smallTalkReply = activity.CreateReply();
                            smallTalkReply.Recipient = activity.From;
                            smallTalkReply.Type = "message";
                            smallTalkReply.Attachments = new List<Attachment>();

                            HeroCard plCard = new HeroCard()
                            {
                                Title = "",
                                Text = smallTalkConfirm_result[smallTalkConfirmNum]
                            };

                            Attachment plAttachment = plCard.ToAttachment();
                            smallTalkReply.Attachments.Add(plAttachment);

                            SetActivity(smallTalkReply);
                            replyresult = "S";
                            db.UserDataUpdate(activity.ChannelId, activity.Conversation.Id, 0);
                        }
                        //건의사항
                        else if ((userData[0].conversationsId == activity.Conversation.Id && (userData[0].loop==1 || userData[0].loop == 2)))
                        {
                            Debug.WriteLine("suggestions dialogue-------------");

                            Activity suggestionsReply = activity.CreateReply();
                            suggestionsReply.Recipient = activity.From;
                            suggestionsReply.Type = "message";
                            suggestionsReply.Attachments = new List<Attachment>();

                            List<TextList> text = new List<TextList>();

                            if (userData[0].loop == 1)
                            {
                                text = db.SelectSuggetionsDialogText("6");
                                db.UserDataUpdate(activity.ChannelId, activity.Conversation.Id, 2);
                                replyresult = "G";
                            }
                            else
                            {
                                text = db.SelectSuggetionsDialogText("7");
                                db.UserDataUpdate(activity.ChannelId, activity.Conversation.Id, 0);
                                replyresult = "G";
                            }

                            for (int i = 0; i < text.Count; i++)
                            {
                                UserHeroCard plCard = new UserHeroCard()
                                {
                                    Title = text[i].cardTitle,
                                    Text = text[i].cardText
                                };

                                Attachment plAttachment = plCard.ToAttachment();
                                suggestionsReply.Attachments.Add(plAttachment);
                            }

                            SetActivity(suggestionsReply);

                        }
                        else if (!qnAMakerAnswer.Equals("No good match found in KB"))
                        {
                            Activity qnAMakerReply = activity.CreateReply();

                            qnAMakerReply.Recipient = activity.From;
                            qnAMakerReply.Type = "message";
                            qnAMakerReply.Attachments = new List<Attachment>();

                            List<CardList> text = new List<CardList>();

                            UserHeroCard plCard = new UserHeroCard()
                            {
                                Title = "SAP 용어",
                                Text = qnAMakerAnswer
                            };

                            Attachment plAttachment = plCard.ToAttachment();
                            qnAMakerReply.Attachments.Add(plAttachment);

                            SetActivity(qnAMakerReply);

                            db.UserDataUpdate(activity.ChannelId, activity.Conversation.Id, 0);
                            replyresult = "Q";
                            luisIntent = "SAP";
                            luistTpyeEntities = "SAP";
                        }
                        else
                        {
                            Debug.WriteLine("no dialogue-------------");

                            Activity intentNoneReply = activity.CreateReply();

                            var message = MessagesController.queryStr;

                            Debug.WriteLine("NO DIALOGUE MESSAGE : " + message);

                            Activity sorryReply = activity.CreateReply();
                            sorryReply.Recipient = activity.From;
                            sorryReply.Type = "message";
                            sorryReply.Attachments = new List<Attachment>();
                            //sorryReply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                            List<CardList> text = new List<CardList>();
                            List<CardAction> cardButtons = new List<CardAction>();

                            text = db.SelectSorryDialogText("5");
                            for (int i = 0; i < text.Count; i++)
                            {
                                CardAction plButton = new CardAction();
                                plButton = new CardAction()
                                {
                                    Type = text[i].btn1Type,
                                    Value = text[i].btn1Context,
                                    Title = text[i].btn1Title
                                };
                                cardButtons.Add(plButton);

                                UserHeroCard plCard = new UserHeroCard()
                                {
                                    //Title = text[i].cardTitle,
                                    Text = text[i].cardText,
                                    Buttons = cardButtons
                                };

                                Attachment plAttachment = plCard.ToAttachment();
                                sorryReply.Attachments.Add(plAttachment);
                            }

                            db.UserDataUpdate(activity.ChannelId, activity.Conversation.Id, 0);

                            SetActivity(sorryReply);
                            replyresult = "D";

                        }

                        DateTime endTime = DateTime.Now;

                        //analysis table insert
                        int dbResult = db.insertUserQuery();

                        //history table insert
                        db.insertHistory(activity.Conversation.Id, activity.ChannelId, ((endTime - MessagesController.startTime).Milliseconds), "", "", "", "", replyresult);
                        replyresult = "";
                        luisIntent = "";
                        luistTpyeEntities = "";
                    }
                }
                catch (Exception e)
                {
                    Debug.Print(e.StackTrace);
                    //int sorryMessageCheck = db.SelectUserQueryErrorMessageCheck(activity.Conversation.Id, MessagesController.chatBotID);

                    Activity sorryReply = activity.CreateReply();

                    sorryReply.Recipient = activity.From;
                    sorryReply.Type = "message";
                    sorryReply.Attachments = new List<Attachment>();
                    //sorryReply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                    List<CardList> text = new List<CardList>();
                    List<CardAction> cardButtons = new List<CardAction>();

                    text = db.SelectSorryDialogText("5");
                    for (int i = 0; i < text.Count; i++)
                    {
                        CardAction plButton = new CardAction();
                        plButton = new CardAction()
                        {
                            Type = text[i].btn1Type,
                            Value = text[i].btn1Context,
                            Title = text[i].btn1Title
                        };
                        cardButtons.Add(plButton);

                        UserHeroCard plCard = new UserHeroCard()
                        {
                            //Title = text[i].cardTitle,
                            Text = text[i].cardText,
                            Buttons = cardButtons
                        };

                        Attachment plAttachment = plCard.ToAttachment();
                        sorryReply.Attachments.Add(plAttachment);
                    }

                    SetActivity(sorryReply);

                    DateTime endTime = DateTime.Now;
                    int dbResult = db.insertUserQuery();
                    db.insertHistory(activity.Conversation.Id, activity.ChannelId, ((endTime - MessagesController.startTime).Milliseconds), luisIntent, luisEntities, luisIntentScore, "","E");
                    replyresult = "";
                    luisIntent = "";
                    luistTpyeEntities = "";
                }
                finally
                {
                    if (reply1.Attachments.Count != 0 || reply1.Text != "")
                    {
                        await connector.Conversations.SendToConversationAsync(reply1);
                    }
                    if (reply2.Attachments.Count != 0 || reply2.Text != "")
                    {
                        await connector.Conversations.SendToConversationAsync(reply2);
                    }
                    if (reply3.Attachments.Count != 0 || reply3.Text != "")
                    {
                        await connector.Conversations.SendToConversationAsync(reply3);
                    }
                    if (reply4.Attachments.Count != 0 || reply4.Text != "")
                    {
                        await connector.Conversations.SendToConversationAsync(reply4);
                    }
                }
            }
            else
            {
                HandleSystemMessage(activity);
            }
            response = Request.CreateResponse(HttpStatusCode.OK);
            return response;

        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
            }
            else if (message.Type == ActivityTypes.Typing)
            {
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }
            return null;
        }


    }
}