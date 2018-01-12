using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnicornWidget.Data;
using UnicornWidget.Models;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;
using Windows.Web.Syndication;
using Unicorn.UWP;

namespace UnicornWidget.Business
{
    public class NotificationManager
    {
        private static string appName = ConfigurationManager.AppSettings.Get("appName");
        private string rssFeedUrl;

        public NotificationManager(string rssFeedUrl)
        {
            this.rssFeedUrl = rssFeedUrl;
            DbInitialization.Initialize();
            
        }

        public void ShowNotifications()
        {
            var toastCollection = GetNotificationsFromRss();
            LogManager.Log("2->toastCollection.Count :" + toastCollection.Count());
            var notifi = ToastNotificationManager.CreateToastNotifier(appName);
            for (int i = 0; i < toastCollection.Count(); i++)
            {
                var unicornToast = toastCollection.ElementAt(i);
                XmlDocument doc = new XmlDocument();
                var xmlString = unicornToast.ToastContent.GetContent();
                doc.LoadXml(xmlString);

                if (i == 0)
                {
                    //MessageBox.Show("before toast");
                    var toast = new ToastNotification(doc);
                    //toast.Activated += Toast_Activated;
                    //toast.Failed += Toast_Failed;
                    LogManager.Log("3->notifi.Show :");
                    notifi.Show(toast);
                    LogManager.Log("3->notifi.Show done :");
                    //MessageBox.Show("after toast");
                    AddNotificationToDb(unicornToast.Url);
                }
                else
                {
                    var scheduleDuration = 4; //???????ConfigurationManager.AppSettings.Get("scheduleDurationGap")
                    var scheduledToast = new ScheduledToastNotification(doc, new DateTimeOffset(DateTime.Now.AddMinutes(i * scheduleDuration)));
                    notifi.AddToSchedule(scheduledToast);
                    LogManager.Log("3->notifi.AddToSchedule :");
                    AddNotificationToDb(unicornToast.Url);
                }
            }
        }

        private void Toast_Failed(ToastNotification sender, ToastFailedEventArgs args)
        {
            LogManager.Log("Toast_Failed : " + args.ErrorCode.StackTrace);
        }

        private void Toast_Activated(ToastNotification sender, object args)
        {
            LogManager.Log("Toast_Activated: " + sender.Content.InnerText);
        }

        public void RemoveScheduledNotifications(int duration)
        {
            var notifi = ToastNotificationManager.CreateToastNotifier(appName);

            foreach (var scheduledNotification in notifi.GetScheduledToastNotifications())
            {
                var xElement = XElement.Parse(scheduledNotification.Content.GetXml());
                var link = xElement.Element("actions").Element("action").Attribute("arguments").Value;
                notifi.RemoveFromSchedule(scheduledNotification);
            }
        }

        //private void Toast_Activated(ToastNotification sender, object args)
        //{
        //    var xElement = XElement.Parse(sender.Content.GetXml());
        //    var link = xElement.Element("actions").Element("action").Attribute("arguments").Value;
        //    AddNotificationToDb(link);
        //}

        private IEnumerable<UnicornToastContent> GetNotificationsFromRss()
        {
            var rssReader = new RssReader(rssFeedUrl);
            var feed = rssReader.GetSyndicationFeed();
            var unicornToastList = new List<UnicornToastContent>();

            if (feed != null)
            {
                foreach (var item in feed.Items)
                {
                    if (!NotificationAlreadyShown(item))
                    {
                        unicornToastList.Add(ConstructToast(item));
                    }
                }
            }
            return unicornToastList;
        }

        private bool NotificationAlreadyShown(SyndicationItem syndicationItem)
        {
            using (SQLiteConnection conn = new SQLiteConnection(AppVariables.DatabaseSource))
            {
                conn.Open();
                string sql = "Select count(*) from TblNotifications WHERE URL = @url";
                SQLiteCommand command = new SQLiteCommand(sql, conn);
                SQLiteParameter param = new SQLiteParameter("@url", syndicationItem.Links.FirstOrDefault().Uri.AbsoluteUri);
                command.Parameters.Add(param);
                var count = int.Parse(command.ExecuteScalar().ToString());

                if (count >= 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private void AddNotificationToDb(string url)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(AppVariables.DatabaseSource))
                {
                    conn.Open();
                    string sql = "INSERT INTO TblNotifications VALUES(@url)";
                    SQLiteCommand command = new SQLiteCommand(sql, conn);
                    SQLiteParameter param = new SQLiteParameter("@url", url);
                    command.Parameters.Add(param);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                LogManager.Log("ex" + ex.StackTrace);
                throw;
            }


        }

        //private void RemoveNotificationFromDb(string url)
        //{
        //    using (SQLiteConnection conn = new SQLiteConnection(DatabaseSource))
        //    {
        //        conn.Open();
        //        string sql = "DELETE FROM TblNotifications WHERE URL=@url";
        //        SQLiteCommand command = new SQLiteCommand(sql, conn);
        //        SQLiteParameter param = new SQLiteParameter("@url", url);
        //        command.Parameters.Add(param);
        //        command.ExecuteNonQuery();
        //    }
        //}

        private UnicornToastContent ConstructToast(SyndicationItem syndicationItem)
        {
            var unicornToastContent = new UnicornToastContent();

            var toastvisual = new ToastVisual();
            var toastActionsCustom = new ToastActionsCustom();
            var path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            var defaultImage = path + "\\Resources\\Unicorn.png";

            if (syndicationItem.ElementExtensions.Where(p => p.OuterName == "image").Count() != 0)
            {
                string imgUrl = syndicationItem.ElementExtensions.Where(p => p.OuterName == "image").First().GetObject<XElement>().Value;
                var localPath = SaveImage(imgUrl);

                toastvisual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children = {
                                      new AdaptiveText() { Text = syndicationItem.Title.Text,HintStyle=AdaptiveTextStyle.Title },
                                      new AdaptiveText() { Text = syndicationItem.Summary.Text,HintStyle=AdaptiveTextStyle.Subtitle },
                                      new AdaptiveImage() { Source = localPath }
                                   },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = defaultImage,
                            HintCrop = ToastGenericAppLogoCrop.Circle
                        }
                    },
                };
            }
            else
            {
                toastvisual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children = {
                                      new AdaptiveText() { Text = syndicationItem.Title.Text,HintStyle=AdaptiveTextStyle.Title },
                                      new AdaptiveText() { Text = syndicationItem.Summary.Text,HintStyle=AdaptiveTextStyle.Subtitle },
                                   },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = defaultImage,
                            HintCrop = ToastGenericAppLogoCrop.Circle
                        }
                    },
                };
            }

            if (syndicationItem.Links.ElementAtOrDefault(1) != null)
            {
                toastActionsCustom = new ToastActionsCustom
                {
                    Buttons = {
                                  {
                                      new ToastButton("Open", syndicationItem.Links.ElementAtOrDefault(1).Uri.AbsoluteUri)
                                      {
                                          ActivationType = ToastActivationType.Protocol
                                      }
                                  },
                                  {
                                      new ToastButtonDismiss()
                                  }
                              }
                };
            }
            else
            {
                toastActionsCustom = new ToastActionsCustom
                {
                    Buttons = {
                                  {
                                      new ToastButtonDismiss()
                                  }
                              }
                };
            }

            unicornToastContent.ToastContent = new ToastContent { Visual = toastvisual, Actions = toastActionsCustom, Scenario = ToastScenario.Reminder };
            unicornToastContent.Url = syndicationItem.Links.First().Uri.AbsoluteUri;

            return unicornToastContent;
        }

        private static string SaveImage(string imageUrl)
        {
            if (!Directory.Exists(AppVariables.AppDataFolder))
            {
                Directory.CreateDirectory(AppVariables.AppDataFolder);
            }

            try
            {
                var fileName = Guid.NewGuid().ToString();
                var filePath = AppVariables.AppDataFolder + "\\" + fileName + ".jpg";

                using (WebClient webClient = new WebClient())
                {
                    byte[] data = webClient.DownloadData(imageUrl);

                    using (MemoryStream mem = new MemoryStream(data))
                    {
                        using (var yourImage = Image.FromStream(mem))
                        {
                            yourImage.Save(filePath, ImageFormat.Jpeg);
                            return filePath;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}