using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Facebook;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using System.IO;

namespace MonsterCam
{
    public partial class Facebook : PhoneApplicationPage
    {
        private FacebookClient client;
        private string token;

        void FaceBookIdentification()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["response_type"] = "token";
            parameters["display"] = "touch";
            parameters["scope"] = "user_photos,publish_stream";
            parameters["redirect_uri"] = "https://www.facebook.com/connect/login_success.html";
            parameters["client_id"] = "";

             client = new FacebookClient();
            Uri uri = client.GetLoginUrl(parameters);
            wb.Navigate(uri);
        }

        void updateAccessToken()
        {
            client = new FacebookClient();

            // Affectation de l'access token existant
            client.AccessToken = token;

            client.GetCompleted += (a, e) =>
                {
                    Dispatcher.BeginInvoke(() =>
                        {
                            if (e.Error == null)
                            {
                                if ((string)e.UserState == "exchange_token")
                                {
                                    JsonObject data = (JsonObject)e.GetResultData();

                                    // Enregistrement du nouvel access token
                                    token = (string)data["access_token"];
                                    App.AddOrUpdateValue("token2", token);

                                }
                            }
                            else
                            {
                                wb.Visibility = Visibility.Visible;
                                FaceBookIdentification();

                            }
                        });
                };

            // Préparation de la requête d'échange d'access token
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["client_id"] = "";
            parameters["client_secret"] = "";
            parameters["grant_type"] = "fb_exchange_token";
            parameters["fb_exchange_token"] = client.AccessToken;

            client.GetAsync("https://graph.facebook.com/oauth/access_token", parameters, "exchange_token");
        }


        public Facebook()
        {
            InitializeComponent();
            if (App.GetValue("token") != null)
            {
                var client = new FacebookClient(App.GetValue("token") as string);
                App.AddOrUpdateValue("token", null);
                Uri uri = client.GetLogoutUrl(new Dictionary<string, object>());
                wb.Navigate(uri);
            }
            token = App.GetValue("token2") as string;
            if (token != null)
            {
                wb.Visibility = Visibility.Collapsed;
                updateAccessToken();
            }
            else
            {
                wb.Visibility = Visibility.Visible;

                FaceBookIdentification();
            }

            


            ApplicationBar.Buttons.Clear();

            ApplicationBarIconButton appUploadButton = new ApplicationBarIconButton(new Uri("data/appbar.upload.rest.png", UriKind.Relative));
            appUploadButton.Text = "upload";
            appUploadButton.Click += (e, s) =>
            {
                try
                {
                    if (token == null)
                    {
                        wb.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ApplicationBar.IsVisible = false;
                        Message.IsEnabled = false;
                        var prog = new ProgressIndicator();
                        prog.IsVisible = true;
                        prog.IsIndeterminate = true;
                        prog.Text = "upload...";

                        SystemTray.SetProgressIndicator(this, prog);
                        postimage();
                    }
                }
                catch (Exception)
                {
                }

            };
            ApplicationBar.Buttons.Add(appUploadButton);


            ApplicationBarIconButton appWallButton = new ApplicationBarIconButton(new Uri("data/fb_wall.png", UriKind.Relative));
            appWallButton.Text = "wall";
            appWallButton.Click += (e, s) =>
            {
                try
                {
                    if (token == null)
                    {
                        wb.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ApplicationBar.IsVisible = false;
                        Message.IsEnabled = false;
                        var prog = new ProgressIndicator();
                        prog.IsVisible = true;
                        prog.IsIndeterminate = true;
                        prog.Text = "upload...";

                        SystemTray.SetProgressIndicator(this, prog);
                        postmessage();
                    }
                }
                catch (Exception)
                {
                }

            };
            ApplicationBar.Buttons.Add(appWallButton);

            var I = (byte[])App.GetValue("image_selected");
            var bmp = new BitmapImage();
            using (MemoryStream stream = new MemoryStream(I))
            {
                bmp.SetSource(stream);
            }
            image.Source = bmp;



        }

        void postmessage()
        {
            var I = (byte[])App.GetValue("image_selected");
            UploadPhoto("me", token, I, true);
        }

        void postimage()
        {
            var I = (byte[])App.GetValue("image_selected");
            UploadPhoto("me", token, I, false);
        }
        public void CreateAlbum(string accessToken, string name/*,EventHandler<FacebookApiEventArgs> ev*/)
        {
            FacebookClient facebookClient = new FacebookClient(accessToken);
            Dictionary<string, object> albumParameters = new Dictionary<string, object>();
            albumParameters.Add("message", "My Album message");
            albumParameters.Add("name", name);
            facebookClient.PostCompleted += (s, ee) =>
                        {
                            JsonObject data = (JsonObject)ee.GetResultData();
                            string id = (string)data["id"];
                            App.AddOrUpdateValue("id", id);
                            Deployment.Current.Dispatcher.BeginInvoke(delegate()
                   {
                       postimage();
                   });
                        };
            facebookClient.PostAsync("/me/albums", albumParameters);

        }

        public void UploadPhoto(string AlbumId, string accessToken, byte[] fileBytes, bool postmesage)
        {
            FacebookClient facebookClient = new FacebookClient(accessToken);

            FacebookMediaObject mediaObject = new FacebookMediaObject
            {
                FileName = Guid.NewGuid() + ".jpg",
                ContentType = "image/jpeg"
            };
            mediaObject.SetValue(fileBytes);
            IDictionary<string, object> upload = new Dictionary<string, object>();
            if (postmesage)
            {
                upload.Add("name", String.Format("MonsterCam {0:yyyyMMdd-HHmmss}.jpg", DateTime.Now));
            }
            else
            {
                upload.Add("name", Message.Text);
            }
            // upload.Add("message", "test sur l'upload d'une photo");
            //upload.Add("caption", "test sur l'upload d'une photo");
            upload.Add("@file.jpg", mediaObject);




            facebookClient.PostCompleted += (s, ee) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                    {
                        if (ee.Error != null)
                        {
                            MessageBox.Show(ee.Error.Message);
                            NavigationService.GoBack();
                        }


                        else
                        {
                            if (postmesage)
                            {
                                try
                                {
                                    JsonObject data = (JsonObject)ee.GetResultData();
                                    string id = (string)data["id"];
                                    var tmp = new FacebookClient(accessToken);
                                    tmp.GetCompleted += (a, b) =>
                                        {
                                            Deployment.Current.Dispatcher.BeginInvoke(delegate()
                                            {
                                                if (b.Error != null)
                                                {
                                                    MessageBox.Show(ee.Error.Message);
                                                    NavigationService.GoBack();
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        JsonObject data2 = (JsonObject)b.GetResultData();
                                                        postMessage(token, (string)data2["source"], (string)data2["link"]);
                                                    }
                                                    catch (Exception exp)
                                                    {
                                                        MessageBox.Show("Facebook error. Check allowed permission");
                                                        NavigationService.GoBack();
                                                    }
                                                }
                                            });

                                        };
                                    tmp.GetAsync(id);
                                }
                                catch (Exception exp)
                                {
                                    MessageBox.Show("Facebook error. Check allowed permission");
                                    NavigationService.GoBack();
                                }
                                // postMessage(token, id);
                            }
                            else
                            {
                                if (App.IsTrial)
                                {
                                    var displayPopup = new CustomDialog("Picture uploaded");

                                    displayPopup.popup.IsOpen = true;
                                    displayPopup.Closed += () =>
                                    {
                                        NavigationService.GoBack();
                                    };
                                }
                                else
                                {
                                    MessageBox.Show("Picture uploaded");
                                    NavigationService.GoBack();
                                }

                            }
                        }

                    });
            };
            facebookClient.PostAsync("/" + AlbumId + "/photos", upload);

        }

        public void postMessage(string accessToken, string name, string link/*,EventHandler<FacebookApiEventArgs> ev*/)
        {
            FacebookClient facebookClient = new FacebookClient(accessToken);
            Dictionary<string, object> albumParameters = new Dictionary<string, object>();

           // albumParameters.Add("caption", "MonsterCam");
            albumParameters.Add("message", Message.Text);

            if (App.IsTrial)
            {
                albumParameters.Add("picture", name);
                albumParameters.Add("link", "http://windowsphone.com/s?appid=8ac6c849-7b2f-4fa4-9be4-7e9d5f3e46a2");
            }
            else
            {
                albumParameters.Add("link", link);
            }
            //  albumParameters.Add("object_attachment", name);
            //albumParameters.Add("attachment", name);


            facebookClient.PostCompleted += (s, ee) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    if (ee.Error != null)
                    {
                        MessageBox.Show(ee.Error.Message);

                    }
                    else
                    {
                        if (App.IsTrial)
                        {
                            var displayPopup = new CustomDialog("Message posted");

                            displayPopup.popup.IsOpen = true;
                            displayPopup.Closed += () =>
                            {
                                NavigationService.GoBack();
                            };
                        }
                        else
                        {
                            MessageBox.Show("Message posted");
                            NavigationService.GoBack();
                        }
                        // MessageBox.Show("Message posted");

                    }




                });
            };
            facebookClient.PostAsync("/me/feed", albumParameters);

        }


        private void wb_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            FacebookOAuthResult result;
            if (client.TryParseOAuthCallbackUrl(e.Uri, out result))
            {
                if (result.IsSuccess)
                {
                    token = result.AccessToken;

                    App.AddOrUpdateValue("token2", token);

                }
                wb.Visibility = Visibility.Collapsed;
            }

        }

        private void wb_NavigationFailed(object sender, System.Windows.Navigation.NavigationFailedEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
            NavigationService.GoBack();
        }
    }
}