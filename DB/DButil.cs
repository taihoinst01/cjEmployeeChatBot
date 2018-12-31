﻿using cjEmployeeChatBot.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using Newtonsoft.Json;

namespace cjEmployeeChatBot.DB
{
    public class DButil
    {
        //DbConnect db = new DbConnect();
        //재시도 횟수 설정
        private static int retryCount = 3;

        //public String GetMultiLUIS(string query)
        //{
        //    //루이스 json 선언
        //    JObject Luis = new JObject();
        //    string LuisName = "";
        //    try
        //    {
        //        int MAX = MessagesController.LUIS_APP_ID.Count(s => s != null);
        //        Array.Resize(ref MessagesController.LUIS_APP_ID, MAX);
        //        Array.Resize(ref MessagesController.LUIS_NM, MAX);

        //        String[] returnLuisName = new string[MAX];
        //        JObject[] Luis_before = new JObject[MAX];

        //        List<string[]> textList = new List<string[]>(MAX);

        //        for (int i = 0; i < MAX; i++)
        //        {
        //            //textList.Add(LUIS_APP_ID[i] +"|"+ LUIS_SUBSCRIPTION + "|" + query);
        //            textList.Add(new string[] { MessagesController.LUIS_NM[i], MessagesController.LUIS_APP_ID[i], MessagesController.LUIS_SUBSCRIPTION, query });
        //            Debug.WriteLine("GetMultiLUIS() LUIS_NM : " + MessagesController.LUIS_NM[i] + " | LUIS_APP_ID : " + MessagesController.LUIS_APP_ID[i]);
        //        }
                
        //        //병렬처리 시간 체크
        //        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        //        watch.Start();
        //        Parallel.For(0, MAX, new ParallelOptions { MaxDegreeOfParallelism = MAX }, async async =>
        //        {
        //            var task_luis = Task<JObject>.Run(() => GetIntentFromBotLUIS(textList[async][1], textList[async][2], textList[async][3]));

        //            try
        //            {
        //                Task.WaitAll(task_luis);

        //                Luis_before[async] = task_luis.Result;
        //                returnLuisName[async] = textList[async][0];

        //            }
        //            catch (AggregateException e)
        //            {
        //                Debug.WriteLine("GetMultiLUIS error = " + e.Message);
        //            }

        //        });

        //        watch.Stop();
        //        //Luis = Luis_before;

        //        //try
        //        //{
        //        //    for (int i = 0; i < MAX; i++)
        //        //    {
        //        //        //엔티티 합치기
        //        //        if ((int)Luis_before[i]["entities"].Count() > 0)
        //        //        {
        //        //            for (int j = 0; j < (int)Luis_before[i]["entities"].Count(); j++)
        //        //            {
        //        //                entitiesSum += (string)Luis_before[i]["entities"][j]["entity"].ToString() + ",";
        //        //            }
        //        //        }

        //        //    }
        //        //}
        //        //catch (IndexOutOfRangeException e)
        //        //{
        //        //    Debug.WriteLine("error = " + e.Message);
        //        //    return "";
        //        //}
        
        //        string luisEntities = "";
        //        string luisIntent = "";
        //        float luisScoreCompare = 0.0f;

        //        //intent score이 제일 큰 intent 추출
        //        if (MAX > 0)
        //        {
        //            for (int i = 0; i < MAX; i++)
        //            {
        //                //entities 0일 경우 PASS
        //                if ((int)Luis_before[i]["entities"].Count() > 0)
        //                {
        //                    //intent None일 경우 PASS
        //                    if (Luis_before[i]["intents"][0]["intent"].ToString() != "None")
        //                    {
        //                        //제한점수 체크
        //                        if ((float)Luis_before[i]["intents"][0]["score"] > Convert.ToDouble(MessagesController.LUIS_SCORE_LIMIT))
        //                        {
        //                            if ((float)Luis_before[i]["intents"][0]["score"] > luisScoreCompare)
        //                            {
        //                                LuisName = returnLuisName[i];
        //                                Luis = Luis_before[i];
        //                                luisScoreCompare = (float)Luis_before[i]["intents"][0]["score"];
        //                                Debug.WriteLine("GetMultiLUIS() LuisName1 : " + LuisName);
        //                            }
        //                            else
        //                            {
        //                                //LuisName = returnLuisName[i];
        //                                //Luis = Luis_before[i];
        //                                Debug.WriteLine("GetMultiLUIS() LuisName2 : " + LuisName);
        //                            }

        //                        }
        //                    }
        //                }   
        //            }

        //            Debug.WriteLine("luisScoreCompare : " + luisScoreCompare);
        //            Debug.WriteLine("LuisName : " + LuisName);
        //        }

        //        //entities 0인것을 intent none으로 변경
        //        Debug.WriteLine("entities====" + (int)Luis["entities"].Count());
        //        if((int)Luis["entities"].Count() != 0)
        //        {
        //            if (!String.IsNullOrEmpty(LuisName))
        //            {
        //                if (Luis != null || Luis.Count > 0)
        //                {
        //                    float luisScore = (float)Luis["intents"][0]["score"];
        //                    int luisEntityCount = (int)Luis["entities"].Count();

        //                    luisIntent = Luis["topScoringIntent"]["intent"].ToString();//add
        //                    luisScore = luisScoreCompare;
        //                    Debug.WriteLine("GetMultiLUIS() LUIS luisIntent : " + luisIntent);

        //                    if (MessagesController.relationList != null)
        //                    {
        //                        Debug.WriteLine("GetMultiLUIS() relationList is not NULL");
        //                        if (MessagesController.relationList.Count() > 0)
        //                        {
        //                            MessagesController.relationList[0].luisScore = (int)Luis["intents"][0]["score"];
        //                        }
        //                        else
        //                        {
        //                            MessagesController.cacheList.luisScore = Luis["intents"][0]["score"].ToString();
        //                        }
        //                    }

        //                    //통근버스
        //                    if (luisIntent.Equals("총무통근버스_통근버스노선안내"))
        //                    {
        //                        for (int i = 0; i < (int)Luis["entities"].Count(); i++)
        //                        {
        //                            if ((string)Luis["entities"][i]["type"] == "L>통근버스노선")
        //                            {
        //                                MessagesController.luistTypeEntities = Regex.Replace((string)Luis["entities"][i]["entity"], " ", "");
        //                            }
        //                        }

        //                    }

        //                    Debug.WriteLine("통근버스노선" + MessagesController.luistTypeEntities);

        //                    /*
        //                    if (luisScore > Convert.ToDouble(MessagesController.LUIS_SCORE_LIMIT) && luisEntityCount > 0)
        //                    {
        //                        Debug.WriteLine("GetMultiLUIS() luisEntityCount > 0");
        //                        for (int i = 0; i < luisEntityCount; i++)
        //                        {
        //                            //luisEntities = luisEntities + Luis["entities"][i]["entity"] + ",";

        //                            //luisType = (string)Luis["entities"][i]["type"];
        //                            //luisType = Regex.Split(luisType, "::")[1];
        //                            //luisEntities = luisEntities + luisType + ",";
        //                        }
        //                    }
        //                    */
        //                }

        //                if (!string.IsNullOrEmpty(luisEntities) || luisEntities.Length > 0)
        //                {
        //                    luisEntities = luisEntities.Substring(0, luisEntities.LastIndexOf(","));
        //                    luisEntities = Regex.Replace(luisEntities, " ", "");


        //                    luisEntities = MessagesController.db.SelectArray(luisEntities);

        //                    if (Luis["intents"] == null)
        //                    {
        //                        MessagesController.cacheList.luisIntent = "";
        //                    }
        //                    else
        //                    {
        //                        MessagesController.cacheList.luisIntent = (string)Luis["intents"][0]["intent"];
        //                    }

        //                    MessagesController.cacheList.luisEntities = luisEntities;
        //                }

        //                //MessagesController.cacheList.luisEntities = LuisName;

        //            }
        //        }
        //        else
        //        {
        //            luisIntent = "None";
        //        }
                

        //        //return LuisName;
        //        return luisIntent;
        //    }
        //    catch (System.Exception e)
        //    {
        //        Debug.WriteLine(e.Message);
        //        return "";
        //    }
        //}

        //private static async Task<JObject> GetIntentFromBotLUIS(string luis_app_id, string luis_subscription, string query)
        //{
        //    JObject jsonObj = new JObject();

        //    query = Uri.EscapeDataString(query);

        //    //string url = string.Format("https://southeastasia.api.cognitive.microsoft.com/luis/v2.0/apps/{0}?subscription-key={1}&timezoneOffset=0&verbose=true&q={2}", luis_app_id, luis_subscription, query);
        //    //string url = string.Format("https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/{0}?subscription-key={1}&timezoneOffset=0&verbose=true&q={2}", luis_app_id, luis_subscription, query);
        //    string url = string.Format("https://eastasia.api.cognitive.microsoft.com/luis/v2.0/apps/{0}?subscription-key={1}&timezoneOffset=0&verbose=true&q={2}", luis_app_id, luis_subscription, query);

            
        //    Debug.WriteLine("-----LUIS URL 보기");
        //    Debug.WriteLine("-----LUIS URL : " + url);

        //    using (HttpClient client = new HttpClient())
        //    {
        //        //취소 시간 설정
        //        client.Timeout = TimeSpan.FromMilliseconds(MessagesController.LUIS_TIME_LIMIT); //3초
        //        var cts = new CancellationTokenSource();
        //        try
        //        {
        //            HttpResponseMessage msg = await client.GetAsync(url, cts.Token);

        //            int currentRetry = 0;

        //            Debug.WriteLine("msg.IsSuccessStatusCode1 = " + msg.IsSuccessStatusCode);
        //            HistoryLog("msg.IsSuccessStatusCode1 = " + msg.IsSuccessStatusCode);

        //            if (msg.IsSuccessStatusCode)
        //            {
        //                var JsonDataResponse = await msg.Content.ReadAsStringAsync();
        //                jsonObj = JObject.Parse(JsonDataResponse);
        //                currentRetry = 0;
        //            }
        //            else
        //            {
        //                //통신장애, 구독만료, url 오류                  
        //                //오류시 3번retry
        //                for (currentRetry = 0; currentRetry < retryCount; currentRetry++)
        //                {
        //                    //테스용 url 설정
        //                    //string url_re = string.Format("https://southeastasia.api.cognitive.microsoft.com/luis/v2.0/apps/{0}?subscription-key={1}&timezoneOffset=0&verbose=true&q={2}", luis_app_id, luis_subscription, query);
        //                    string url_re = string.Format("https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/{0}?subscription-key={1}&timezoneOffset=0&verbose=true&q={2}", luis_app_id, luis_subscription, query);
        //                    HttpResponseMessage msg_re = await client.GetAsync(url_re, cts.Token);

        //                    if (msg_re.IsSuccessStatusCode)
        //                    {
        //                        //다시 호출
        //                        Debug.WriteLine("msg.IsSuccessStatusCode2 = " + msg_re.IsSuccessStatusCode);
        //                        HistoryLog("msg.IsSuccessStatusCode2 = " + msg.IsSuccessStatusCode);
        //                        var JsonDataResponse = await msg_re.Content.ReadAsStringAsync();
        //                        jsonObj = JObject.Parse(JsonDataResponse);
        //                        currentRetry = 0;
        //                        break;
        //                    }
        //                    else
        //                    {
        //                        //초기화
        //                        //jsonObj = JObject.Parse(@"{
        //                        //    'query':'',
        //                        //    'topScoringIntent':0,
        //                        //    'intents':[],
        //                        //    'entities':'[]'
        //                        //}");
        //                        Debug.WriteLine("GetIntentFromBotLUIS else print ");
        //                        HistoryLog("GetIntentFromBotLUIS else print ");
        //                        jsonObj = JObject.Parse(@"{
        //                                                              'query': '',
        //                                                              'topScoringIntent': {
        //                                                                'intent': 'None',
        //                                                                'score': 0.09
        //                                                              },
        //                                                              'intents': [
        //                                                                {
        //                                                                  'intent': 'None',
        //                                                                  'score': 0.09
        //                                                                }
        //                                                              ],
        //                                                              'entities': []
        //                                                            }
        //                                                            ");
        //                    }
        //                }
        //            }

        //            msg.Dispose();
        //        }
        //        catch (TaskCanceledException e)
        //        {
        //            Debug.WriteLine("GetIntentFromBotLUIS error = " + e.Message);
        //            HistoryLog("GetIntentFromBotLUIS error = " + e.Message);
        //            //초기화
        //            //jsonObj = JObject.Parse(@"{
        //            //                'query':'',
        //            //                'topScoringIntent':0,
        //            //                'intents':[],
        //            //                'entities':'[]'
        //            //            }");

        //            jsonObj = JObject.Parse(@"{
        //                                                  'query': '',
        //                                                  'topScoringIntent': {
        //                                                    'intent': 'None',
        //                                                    'score': 0.09
        //                                                  },
        //                                                  'intents': [
        //                                                    {
        //                                                      'intent': 'None',
        //                                                      'score': 0.09
        //                                                    }
        //                                                  ],
        //                                                  'entities': []
        //                                                }
        //                                                ");

        //        }
        //    }
        //    return jsonObj;
        //}

        public static void HistoryLog(String strMsg)
        {
            try
            {
                //Debug.WriteLine("AppDomain.CurrentDomain.BaseDirectory : " + AppDomain.CurrentDomain.BaseDirectory);
                string m_strLogPrefix = AppDomain.CurrentDomain.BaseDirectory + @"LOG\";
                string m_strLogExt = @".LOG";
                DateTime dtNow = DateTime.Now;
                string strDate = dtNow.ToString("yyyy-MM-dd");
                string strPath = String.Format("{0}{1}{2}", m_strLogPrefix, strDate, m_strLogExt);
                string strDir = Path.GetDirectoryName(strPath);
                DirectoryInfo diDir = new DirectoryInfo(strDir);

                if (!diDir.Exists)
                {
                    diDir.Create();
                    diDir = new DirectoryInfo(strDir);
                }

                if (diDir.Exists)
                {
                    System.IO.StreamWriter swStream = File.AppendText(strPath);
                    string strLog = String.Format("{0}: {1}", dtNow.ToString("MM/dd/yyyy hh:mm:ss.fff"), strMsg);
                    swStream.WriteLine(strLog);
                    swStream.Close(); ;
                }
            }
            catch (System.Exception e)
            {
                HistoryLog(e.Message);
            }
        }


        public Attachment getAttachmentFromDialog(DialogList dlg, Activity activity)
        {
            Attachment returnAttachment = new Attachment();
            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            if (dlg.dlgType.Equals(MessagesController.TEXTDLG))
            {

                if (!activity.ChannelId.Equals("facebook"))
                {
                    UserHeroCard plCard = new UserHeroCard()
                    {
                        Title = dlg.cardTitle,
                        Text = dlg.cardText,
                        Gesture = dlg.gesture
                    };
                    returnAttachment = plCard.ToAttachment();
                }

                
            }
            else if (dlg.dlgType.Equals(MessagesController.MEDIADLG))
            {

                string cardDiv = "";
                string cardVal = "";

                List<CardImage> cardImages = new List<CardImage>();
                List<CardAction> cardButtons = new List<CardAction>();

                HistoryLog("CARD IMG START");
                if (dlg.mediaUrl != null)
                {
                    HistoryLog("FB CARD IMG " + dlg.mediaUrl);
                    cardImages.Add(new CardImage(url: dlg.mediaUrl));
                }


                HistoryLog("CARD BTN1 START");
                if (activity.ChannelId.Equals("facebook") && dlg.btn1Type == null && !string.IsNullOrEmpty(dlg.cardDivision) && dlg.cardDivision.Equals("play") && !string.IsNullOrEmpty(dlg.cardValue))
                {
                    CardAction plButton = new CardAction();
                    plButton = new CardAction()
                    {
                        Value = dlg.cardValue,
                        Type = "openUrl",
                        Title = "영상보기"
                    };
                    cardButtons.Add(plButton);
                }
                else if (dlg.btn1Type != null)
                {
                    CardAction plButton = new CardAction();
                    plButton = new CardAction()
                    {
                        Value = dlg.btn1Context,
                        Type = dlg.btn1Type,
                        Title = dlg.btn1Title
                    };
                    cardButtons.Add(plButton);
                }

                if (dlg.btn2Type != null)
                {
                    if (!(activity.ChannelId == "facebook" && dlg.btn2Title == "나에게 맞는 모델 추천"))
                    {
                        CardAction plButton = new CardAction();
                        HistoryLog("CARD BTN2 START");
                        plButton = new CardAction()
                        {
                            Value = dlg.btn2Context,
                            Type = dlg.btn2Type,
                            Title = dlg.btn2Title
                        };
                        cardButtons.Add(plButton);
                    }
                }

                if (dlg.btn3Type != null )
                {
                    
                    CardAction plButton = new CardAction();

                    HistoryLog("CARD BTN3 START");
                    plButton = new CardAction()
                    {
                        Value = dlg.btn3Context,
                        Type = dlg.btn3Type,
                        Title = dlg.btn3Title
                    };
                    cardButtons.Add(plButton);
                    
                }

                if (dlg.btn4Type != null)
                {
                    CardAction plButton = new CardAction();
                    HistoryLog("CARD BTN4 START");
                    plButton = new CardAction()
                    {
                        Value = dlg.btn4Context,
                        Type = dlg.btn4Type,
                        Title = dlg.btn4Title
                    };
                    cardButtons.Add(plButton);
                }

                if (!string.IsNullOrEmpty(dlg.cardDivision))
                {
                    cardDiv = dlg.cardDivision;
                }

                if (!string.IsNullOrEmpty(dlg.cardValue))
                {
                    //cardVal = priceMediaDlgList[i].cardValue.Replace();
                    cardVal = dlg.cardValue;
                }
                //HistoryLog("!!!!!FB CARD BTN1 START channelID.Equals(facebook) && cardButtons.Count < 1 && cardImages.Count < 1");
                HeroCard plCard = new UserHeroCard();
                if (activity.ChannelId == "facebook" && string.IsNullOrEmpty(dlg.cardTitle))
                {
                    //HistoryLog("FB CARD BTN1 START channelID.Equals(facebook) && string.IsNullOrEmpty(card.cardTitle)");
                    plCard = new UserHeroCard()
                    {
                        Title = "선택해 주세요",
                        Text = dlg.cardText,
                        Images = cardImages,
                        Buttons = cardButtons,
                        Card_division = cardDiv,
                        Card_value = cardVal
                    };
                    returnAttachment = plCard.ToAttachment();
                }
                else if (activity.ChannelId == "facebook" && string.IsNullOrEmpty(dlg.cardValue))
                {
                    //HistoryLog("FB CARD BTN1 START channelID.Equals(facebook) && string.IsNullOrEmpty(card.cardValue)");
                    plCard = new UserHeroCard()
                    {
                        Title = dlg.cardTitle,
                        Images = cardImages,
                        Buttons = cardButtons
                    };
                    returnAttachment = plCard.ToAttachment();
                }
                else
                {
                    //HistoryLog("!!!!!!!!FB CARD BTN1 START channelID.Equals(facebook) && string.IsNullOrEmpty(card.cardTitle)");
                    plCard = new UserHeroCard()
                    {
                        Title = dlg.cardTitle,
                        Text = dlg.cardText,
                        Images = cardImages,
                        Buttons = cardButtons,
                        Card_division = cardDiv,
                        Card_value = cardVal
                    };
                    returnAttachment = plCard.ToAttachment();
                }
            }
            else
            {
                Debug.WriteLine("Dialog Type Error : " + dlg.dlgType);
            }
            return returnAttachment;
        }


        public Attachment getAttachmentFromDialog(CardList card, Activity activity, string userSSO)
        {
            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            Attachment returnAttachment = new Attachment();

            string cardDiv = "";
            string cardVal = "";

            List<CardImage> cardImages = new List<CardImage>();
            List<CardAction> cardButtons = new List<CardAction>();
            //HistoryLog("CARD IMG START");
            if (card.imgUrl != null)
            {
                HistoryLog("FB CARD IMG " + card.imgUrl);
                cardImages.Add(new CardImage(url: card.imgUrl));
            }


            //HistoryLog("CARD BTN1 START");

            if (!userSSO.Equals("INIT"))
            {
                card = chkOpenUrlDlg(card, userSSO);
            }

            if (activity.ChannelId.Equals("facebook") && card.btn1Type == null && !string.IsNullOrEmpty(card.cardDivision) && card.cardDivision.Equals("play") && !string.IsNullOrEmpty(card.cardValue))
            {
                CardAction plButton = new CardAction();
                plButton = new CardAction()
                {
                    Value = card.cardValue,
                    Type = "openUrl",
                    Title = "영상보기"
                };
                cardButtons.Add(plButton);
            }
            else if (card.btn1Type != null)
            {
                CardAction plButton = new CardAction();
                plButton = new CardAction()
                {
                    Value = card.btn1Context,
                    Type = card.btn1Type,
                    Title = card.btn1Title
                };
                cardButtons.Add(plButton);
            }

            if (card.btn2Type != null)
            {
                CardAction plButton = new CardAction();
                //HistoryLog("CARD BTN2 START");
                plButton = new CardAction()
                {
                    Value = card.btn2Context,
                    Type = card.btn2Type,
                    Title = card.btn2Title
                };
                cardButtons.Add(plButton);
            }

            if (card.btn3Type != null)
            {
                CardAction plButton = new CardAction();

                //HistoryLog("CARD BTN3 START");
                plButton = new CardAction()
                {
                    Value = card.btn3Context,
                    Type = card.btn3Type,
                    Title = card.btn3Title
                };
                cardButtons.Add(plButton);
            }

            if (card.btn4Type != null)
            {
                CardAction plButton = new CardAction();
                //HistoryLog("CARD BTN4 START");
                plButton = new CardAction()
                {
                    Value = card.btn4Context,
                    Type = card.btn4Type,
                    Title = card.btn4Title
                };
                cardButtons.Add(plButton);
            }



            if (!string.IsNullOrEmpty(card.cardDivision))
            {
                cardDiv = card.cardDivision;
            }

            if (!string.IsNullOrEmpty(card.cardValue))
            {
                //cardVal = priceMediaDlgList[i].cardValue.Replace();
                cardVal = card.cardValue;
            }


            if(activity.ChannelId.Equals("facebook") && cardButtons.Count < 1 && cardImages.Count < 1)
            {
                //HistoryLog("FB CARD BTN1 START channelID.Equals(facebook) && cardButtons.Count < 1 && cardImages.Count < 1");
                Activity reply_facebook = activity.CreateReply();
                reply_facebook.Recipient = activity.From;
                reply_facebook.Type = "message";
                //HistoryLog("facebook  card Text : " + card.cardText);
                reply_facebook.Text = card.cardText;
                var reply_ment_facebook = connector.Conversations.SendToConversationAsync(reply_facebook);
            }
            else
            {
                //HistoryLog("!!!!!FB CARD BTN1 START channelID.Equals(facebook) && cardButtons.Count < 1 && cardImages.Count < 1");
                HeroCard plCard = new UserHeroCard();
                if (activity.ChannelId == "facebook" && string.IsNullOrEmpty(card.cardValue))
                {
                    //HistoryLog("FB CARD BTN1 START channelID.Equals(facebook) && string.IsNullOrEmpty(card.cardValue)");
                    plCard = new UserHeroCard()
                    {
                        Title = card.cardTitle,
                        Images = cardImages,
                        Buttons = cardButtons,
                        Gesture = card.gesture //2018-04-24 : 제스처 추가
                    };
                    returnAttachment = plCard.ToAttachment();
                }
                else
                {
                    //HistoryLog("!!!!!!!FB CARD BTN1 START channelID.Equals(facebook) && string.IsNullOrEmpty(card.cardValue)");
                    if (activity.ChannelId == "facebook" && string.IsNullOrEmpty(card.cardTitle))
                    {
                        //HistoryLog("FB CARD BTN1 START channelID.Equals(facebook) && string.IsNullOrEmpty(card.cardTitle)");
                        plCard = new UserHeroCard()
                        {
                            Title = "선택해 주세요",
                            Text = card.cardText,
                            Images = cardImages,
                            Buttons = cardButtons,
                            Card_division = cardDiv,
                            Card_value = cardVal,
                            Gesture = card.gesture //2018-04-24 : 제스처 추가
                        };
                        returnAttachment = plCard.ToAttachment();
                    }
                    else
                    {
                        //HistoryLog("!!!!!!!!FB CARD BTN1 START channelID.Equals(facebook) && string.IsNullOrEmpty(card.cardTitle)");
                        plCard = new UserHeroCard()
                        {
                            Title = card.cardTitle,
                            Text = card.cardText,
                            Images = cardImages,
                            Buttons = cardButtons,
                            Card_division = cardDiv,
                            Card_value = cardVal,
                            Gesture = card.gesture //2018-04-24 : 제스처 추가
                        };
                        returnAttachment = plCard.ToAttachment();
                    }
                    
                }
            }

            return returnAttachment;
        }


        public static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, /*CardAction cardAction*/ List<CardAction> buttons)
        {
            var heroCard = new UserHeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Buttons = buttons,
            };

            return heroCard.ToAttachment();
        }
        public Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, /*CardAction cardAction*/ List<CardAction> buttons, string cardDivision, string cardValue)
        {
            var heroCard = new UserHeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Buttons = buttons,
                Card_division = cardDivision,
                Card_value = cardValue,

            };

            return heroCard.ToAttachment();
        }
        //지도 맵 추가
        public static Attachment GetHeroCard_Map(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction /*List<CardAction> buttons*/, string latitude, string longitude)
        {
            var heroCard = new UserHeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Buttons = new List<CardAction>() { cardAction },
                Latitude = latitude,
                Longitude = longitude,
            };

            return heroCard.ToAttachment();
        }

        //현재 위치 이미지 저장
        //clientId 및 URL 네이버개발자센터에서 확인 및 수정
        public static void mapSave(string url1, string url2)
        {
            //로컬테스트
            //string url = "https://openapi.naver.com/v1/map/staticmap.bin?clientId=dXUekyWEBhyYa2zD2s33&url=file:///C:/Users/user/Desktop&crs=EPSG:4326&center=" + url2 + "," + url1 + "&level=10&w=320&h=320&baselayer=default&markers="+ url2 +"," + url1;
            //웹테스트
            string url = "https://openapi.naver.com/v1/map/staticmap.bin?clientId=dXUekyWEBhyYa2zD2s33&url=https://cjEmployeeChatBot.azurewebsites.net&crs=EPSG:4326&center=" + url2 + "," + url1 + "&level=10&w=320&h=320&baselayer=default&markers=" + url2 + "," + url1;

            System.Drawing.Image image = DownloadImageFromUrl(url);

            string m_strLogPrefix = AppDomain.CurrentDomain.BaseDirectory + @"image\map\";
            string m_strLogExt = @".png";
            string strPath = String.Format("{0}{1}", m_strLogPrefix, m_strLogExt);
            string strDir = Path.GetDirectoryName(strPath);
            DirectoryInfo diDir = new DirectoryInfo(strDir);

            //파일 있는지 확인 있을때(true), 없으면(false)
            FileInfo fileInfo = new FileInfo(strPath + url2 + "," + url1 + ".png");

            if (!fileInfo.Exists)
            {
                string fileName = System.IO.Path.Combine(strDir, url2 +","+ url1+".png");
                try
                {
                    image.Save(fileName);
                } catch(Exception ex)
                {
                    Debug.WriteLine("***error***" + ex.Message);
                }
            }

        }

        public static System.Drawing.Image DownloadImageFromUrl(string imageUrl)
        {
            System.Drawing.Image image = null;

            try
            {
                System.Net.HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(imageUrl);
                webRequest.AllowWriteStreamBuffering = true;
                webRequest.Timeout = 30000;

                System.Net.WebResponse webResponse = webRequest.GetResponse();

                System.IO.Stream stream = webResponse.GetResponseStream();

                image = System.Drawing.Image.FromStream(stream);

                webResponse.Close();
            }
            catch (Exception ex)
            {
                return null;
            }

            return image;
        }

        public String GetQnAMaker(string query)
        {
            var task = Task<string>.Run(() => GetQnAMakerBot(query));
            var msg = (string)task.Result;
            return msg;
        }

        public static async Task<string> GetQnAMakerBot(string query)
        {
            //QnAMaker
            var url =
               "https://cjsapqna.azurewebsites.net/qnamaker/knowledgebases/de5cd645-059a-4e0b-b2a6-d084240d31a8/generateAnswer";
            var httpContent = new StringContent("{'question':'" + query + "'}", Encoding.UTF8, "application/json");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", "EndpointKey a5379adc-3395-431b-bfd4-ef53017e3323");
            var httpResponse = await httpClient.PostAsync(url, httpContent);
            var httpResponseMessage = await httpResponse.Content.ReadAsStringAsync();
            dynamic httpResponseJson = JsonConvert.DeserializeObject(httpResponseMessage);
            //var replyMessage = (string)httpResponseJson.answers[0].answer;
            var replyMessage = "";
            //점수제한
            if (httpResponseJson.answers[0].score > 50.00)
            {
                replyMessage = httpResponseJson.answers[0].answer;
            } else
            {
                replyMessage = "No good match";
            }            

            return replyMessage;           

        }

        //SSO 관련
        public String GetSSO(string query)
        {
            var task = Task<string>.Run(() => GetSSORef(query));
            var msg = (string)task.Result;
            return msg;
        }

        public static async Task<string> GetSSORef(string id)
        {
            var url = "";
            if (id.Substring(0,1) == "M")
            {
                url = "https://cjemployeeconnect3.azurewebsites.net?M=" + id.Replace("Msso:", "");
            }
            else 
            {
                url = "https://cjemployeeconnect3.azurewebsites.net?P=" + id.Replace("Psso:", "");
            }
            //Debug.WriteLine("url");
            //HistoryLog("sso url====" + url);
            var httpClient = new HttpClient();
            var httpResponse = await httpClient.GetAsync(url);
            var httpResponseMessage = await httpResponse.Content.ReadAsStringAsync();

            return httpResponseMessage;
        }

        //SAP 비밀번호 초기화 관련
        public String GetSapInit(string query)
        {
            var task = Task<string>.Run(() => GetSapInitRef(query));
            var msg = (string)task.Result;
            return msg;
        }

        public static async Task<string> GetSapInitRef(string id)
        {
            var url = "";

            url = "https://cjemployeeconnect3.azurewebsites.net?T="+ id;
            HistoryLog("url==" + url);
            var httpClient = new HttpClient();
            var httpResponse = await httpClient.GetAsync(url);
            var httpResponseMessage = await httpResponse.Content.ReadAsStringAsync();

            return httpResponseMessage;
        }

        public CardList chkOpenUrlDlg(CardList inputCardList, string userSSO)
        {
            HistoryLog("userSSO == " + userSSO);
            if (inputCardList.btn1Type != null && inputCardList.btn1Type.Equals("openUrl"))
            {
                inputCardList.btn1Context = chkUrlStr(inputCardList.btn1Context, userSSO);
            }
            if (inputCardList.btn2Type != null && inputCardList.btn2Type.Equals("openUrl"))
            {
                inputCardList.btn2Context = chkUrlStr(inputCardList.btn2Context, userSSO);
            }
            if (inputCardList.btn3Type != null && inputCardList.btn3Type.Equals("openUrl"))
            {
                inputCardList.btn3Context = chkUrlStr(inputCardList.btn3Context, userSSO);
            }
            if (inputCardList.btn4Type != null && inputCardList.btn4Type.Equals("openUrl"))
            {
                inputCardList.btn4Context = chkUrlStr(inputCardList.btn4Context, userSSO);
            }

            return inputCardList;
        }

        public string chkUrlStr(string btnContext, string userSSO)
        {
            string returnStr = btnContext;
            //총무도움방
            if (btnContext.Contains("https://cjemployeechatbot-web.azurewebsites.net") && btnContext.Contains("&cjworld_id="))
            {
                returnStr += userSSO;
            }
            //정보보호
            else if (btnContext.Contains("http://itsecu.cj.net/ism_back/common/sso/ismMain.fo") && btnContext.Contains("&cjworld_id="))
            {
                returnStr += userSSO;
            }
            return returnStr;
        }
    }
}