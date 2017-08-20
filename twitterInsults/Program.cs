using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Web.Script.Serialization;


namespace twitterInsults
{
    class Program
    {


         static string most_recent_tweet_responded = null;

        static void Main(string[] args)
        {
            //check every 15 minutes
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(15);

            var timer = new System.Threading.Timer((e) =>
            {

                tweetInsult();

            }, null, startTimeSpan, periodTimeSpan);

            
            Console.ReadKey();
        }

        static void tweetInsult()
        {
          

                // You need to set your own keys and screen name
                var oAuthConsumerKey = "[Your consumer Key]";
                var oAuthConsumerSecret = "[Your consumer secret]";
                var oAuthAccessToken = "[your access token]";
                var oAuthAccessTokenSecret = "[your access token secret";
                var cb_key = "[your cleverbot key]";

                var oAuthUrl = "https://api.twitter.com/oauth2/token";
                var screenname = "realdonaldtrump";

                // Do the Authenticate
                var authHeaderFormat = "Basic {0}";

                var authHeader = string.Format(authHeaderFormat,
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(Uri.EscapeDataString(oAuthConsumerKey) + ":" +
                    Uri.EscapeDataString((oAuthConsumerSecret)))
                ));

                var postBody = "grant_type=client_credentials";

                HttpWebRequest authRequest = (HttpWebRequest)WebRequest.Create(oAuthUrl);
                authRequest.Headers.Add("Authorization", authHeader);
                authRequest.Method = "POST";
                authRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
                authRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (Stream stream = authRequest.GetRequestStream())
                {
                    byte[] content = ASCIIEncoding.ASCII.GetBytes(postBody);
                    stream.Write(content, 0, content.Length);
                }

                authRequest.Headers.Add("Accept-Encoding", "gzip");

                WebResponse authResponse = authRequest.GetResponse();
                // deserialize into an object
                TwitAuthenticateResponse twitAuthResponse;
                using (authResponse)
                {
                    using (var reader = new StreamReader(authResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objectText = reader.ReadToEnd();
                        twitAuthResponse = JsonConvert.DeserializeObject<TwitAuthenticateResponse>(objectText);
                    }
                }

                // Do the timeline
                var timelineFormat = "https://api.twitter.com/1.1/statuses/user_timeline.json?screen_name={0}&include_rts=1&exclude_replies=1&count=5";
                var timelineUrl = string.Format(timelineFormat, screenname);
                HttpWebRequest timeLineRequest = (HttpWebRequest)WebRequest.Create(timelineUrl);
                var timelineHeaderFormat = "{0} {1}";
                timeLineRequest.Headers.Add("Authorization", string.Format(timelineHeaderFormat, twitAuthResponse.token_type, twitAuthResponse.access_token));
                timeLineRequest.Method = "Get";
                WebResponse timeLineResponse = timeLineRequest.GetResponse();
                var timeLineJson = string.Empty;
                List<Tweet> tweets = new List<Tweet>();

                using (timeLineResponse)
                {
                    using (var reader = new StreamReader(timeLineResponse.GetResponseStream()))
                    {
                        timeLineJson = reader.ReadToEnd();

                    }
                }

            try
            {
                //compile a list of most recent tweets
                var root = new JavaScriptSerializer().Deserialize<List<Tweet>>(timeLineJson);
                List<Tweet> t = new List<Tweet>();
                foreach (var tweet in root)
                {
                    t.Add(tweet);
                }

                //check if most recent == last tweeted at id
                if (most_recent_tweet_responded != t[0].id_str)
                {



                    //pass reply to cleverbot
                    
                    var cb_URL = "http://www.cleverbot.com/getreply?key=" + cb_key + "&input=" + t[0].text + "&cs=76nxdxIJ02AAA";
                    HttpWebRequest cbrequest = (HttpWebRequest)WebRequest.Create(cb_URL);
                    WebResponse cbresponse = cbrequest.GetResponse();
                    var cbJson = string.Empty;

                    using (cbresponse)
                    {

                        using (var cb_reader = new StreamReader(cbresponse.GetResponseStream()))
                        {
                            cbJson = cb_reader.ReadToEnd();

                        }

                    }
                    CleverBotResponse cb = new JavaScriptSerializer().Deserialize<CleverBotResponse>(cbJson);
                    
                    most_recent_tweet_responded = t[0].id_str;

                    //post response
                    var twitter = new TwitterApi(oAuthConsumerKey, oAuthConsumerSecret, oAuthAccessToken, oAuthAccessTokenSecret);
                    var response = twitter.Tweet("@realdonaldtrump " + cb.clever_output, t[0].id_str);
                    Console.WriteLine("DT Tweeted: " + t[0].text + " | " + " response: " + cb.clever_output  + " respondedID: " + most_recent_tweet_responded + " at + " + DateTime.Now.ToString() + "\r\n");
                }

                else
                {
                    Console.WriteLine("No New Tweets at " + DateTime.Now.ToString());

                }

            }
            catch (Exception e)
            {

                Console.WriteLine(e.ToString());

            }

            }
            
        }
        public class Insults
        {
            public string range { get; set; }
            public string majorDimension { get; set; }
            public List<List<string>> values { get; set; }
        }

        public class TwitAuthenticateResponse
        {
            public string token_type { get; set; }
            public string access_token { get; set; }
        }
        public class Entities
        {
            public List<object> hashtags { get; set; }
            public List<object> symbols { get; set; }
            public List<object> user_mentions { get; set; }
            public List<object> urls { get; set; }
        }

        public class Description
        {
            public List<object> urls { get; set; }
        }

        public class Entities2
        {
            public Description description { get; set; }
        }

        public class User
        {
            public int id { get; set; }
            public string id_str { get; set; }
            public string name { get; set; }
            public string screen_name { get; set; }
            public string location { get; set; }
            public string description { get; set; }
            public object url { get; set; }
            public Entities2 entities { get; set; }
            public bool @protected { get; set; }
            public int followers_count { get; set; }
            public int friends_count { get; set; }
            public int listed_count { get; set; }
            public string created_at { get; set; }
            public int favourites_count { get; set; }
            public int utc_offset { get; set; }
            public string time_zone { get; set; }
            public bool geo_enabled { get; set; }
            public bool verified { get; set; }
            public int statuses_count { get; set; }
            public string lang { get; set; }
            public bool contributors_enabled { get; set; }
            public bool is_translator { get; set; }
            public bool is_translation_enabled { get; set; }
            public string profile_background_color { get; set; }
            public string profile_background_image_url { get; set; }
            public string profile_background_image_url_https { get; set; }
            public bool profile_background_tile { get; set; }
            public string profile_image_url { get; set; }
            public string profile_image_url_https { get; set; }
            public string profile_banner_url { get; set; }
            public string profile_link_color { get; set; }
            public string profile_sidebar_border_color { get; set; }
            public string profile_sidebar_fill_color { get; set; }
            public string profile_text_color { get; set; }
            public bool profile_use_background_image { get; set; }
            public bool has_extended_profile { get; set; }
            public bool default_profile { get; set; }
            public bool default_profile_image { get; set; }
            public object following { get; set; }
            public object follow_request_sent { get; set; }
            public object notifications { get; set; }
            public string translator_type { get; set; }
        }

        public class UserMention
        {
            public string screen_name { get; set; }
            public string name { get; set; }
            public int id { get; set; }
            public string id_str { get; set; }
            public List<int> indices { get; set; }
        }

        public class Entities3
        {
            public List<object> hashtags { get; set; }
            public List<object> symbols { get; set; }
            public List<UserMention> user_mentions { get; set; }
            public List<object> urls { get; set; }
        }

        public class Url2
        {
            public string url { get; set; }
            public string expanded_url { get; set; }
            public string display_url { get; set; }
            public List<int> indices { get; set; }
        }

        public class Url
        {
            public List<Url2> urls { get; set; }
        }

        public class Description2
        {
            public List<object> urls { get; set; }
        }

        public class Entities4
        {
            public Url url { get; set; }
            public Description2 description { get; set; }
        }

        public class User2
        {
            public int id { get; set; }
            public string id_str { get; set; }
            public string name { get; set; }
            public string screen_name { get; set; }
            public string location { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public Entities4 entities { get; set; }
            public bool @protected { get; set; }
            public int followers_count { get; set; }
            public int friends_count { get; set; }
            public int listed_count { get; set; }
            public string created_at { get; set; }
            public int favourites_count { get; set; }
            public int utc_offset { get; set; }
            public string time_zone { get; set; }
            public bool geo_enabled { get; set; }
            public bool verified { get; set; }
            public int statuses_count { get; set; }
            public string lang { get; set; }
            public bool contributors_enabled { get; set; }
            public bool is_translator { get; set; }
            public bool is_translation_enabled { get; set; }
            public string profile_background_color { get; set; }
            public string profile_background_image_url { get; set; }
            public string profile_background_image_url_https { get; set; }
            public bool profile_background_tile { get; set; }
            public string profile_image_url { get; set; }
            public string profile_image_url_https { get; set; }
            public string profile_banner_url { get; set; }
            public string profile_link_color { get; set; }
            public string profile_sidebar_border_color { get; set; }
            public string profile_sidebar_fill_color { get; set; }
            public string profile_text_color { get; set; }
            public bool profile_use_background_image { get; set; }
            public bool has_extended_profile { get; set; }
            public bool default_profile { get; set; }
            public bool default_profile_image { get; set; }
            public object following { get; set; }
            public object follow_request_sent { get; set; }
            public object notifications { get; set; }
            public string translator_type { get; set; }
        }

        public class RetweetedStatus
        {
            public string created_at { get; set; }
            public object id { get; set; }
            public string id_str { get; set; }
            public string text { get; set; }
            public bool truncated { get; set; }
            public Entities3 entities { get; set; }
            public string source { get; set; }
            public long? in_reply_to_status_id { get; set; }
            public string in_reply_to_status_id_str { get; set; }
            public int? in_reply_to_user_id { get; set; }
            public string in_reply_to_user_id_str { get; set; }
            public string in_reply_to_screen_name { get; set; }
            public User2 user { get; set; }
            public object geo { get; set; }
            public object coordinates { get; set; }
            public object place { get; set; }
            public object contributors { get; set; }
            public bool is_quote_status { get; set; }
            public int retweet_count { get; set; }
            public int favorite_count { get; set; }
            public bool favorited { get; set; }
            public bool retweeted { get; set; }
            public string lang { get; set; }
        }

        public class Tweet
        {
            public string created_at { get; set; }
            public object id { get; set; }
            public string id_str { get; set; }
            public string text { get; set; }
            public bool truncated { get; set; }
            public Entities entities { get; set; }
            public string source { get; set; }
            public object in_reply_to_status_id { get; set; }
            public object in_reply_to_status_id_str { get; set; }
            public object in_reply_to_user_id { get; set; }
            public object in_reply_to_user_id_str { get; set; }
            public object in_reply_to_screen_name { get; set; }
            public User user { get; set; }
            public object geo { get; set; }
            public object coordinates { get; set; }
            public object place { get; set; }
            public object contributors { get; set; }
            public bool is_quote_status { get; set; }
            public int retweet_count { get; set; }
            public int favorite_count { get; set; }
            public bool favorited { get; set; }
            public bool retweeted { get; set; }
            public bool possibly_sensitive { get; set; }
            public string lang { get; set; }
            public RetweetedStatus retweeted_status { get; set; }
        }
    public class CleverBotResponse
    {
        public string cs { get; set; }
        public string interaction_count { get; set; }
        public string input { get; set; }
        public string input_other { get; set; }
        public string input_label { get; set; }
        public string predicted_input { get; set; }
        public string accuracy { get; set; }
        public string output_label { get; set; }
        public string output { get; set; }
        public string conversation_id { get; set; }
        public string errorline { get; set; }
        public string database_version { get; set; }
        public string software_version { get; set; }
        public string time_taken { get; set; }
        public string random_number { get; set; }
        public string time_second { get; set; }
        public string time_minute { get; set; }
        public string time_hour { get; set; }
        public string time_day_of_week { get; set; }
        public string time_day { get; set; }
        public string time_month { get; set; }
        public string time_year { get; set; }
        public string reaction { get; set; }
        public string reaction_tone { get; set; }
        public string emotion { get; set; }
        public string emotion_tone { get; set; }
        public string clever_accuracy { get; set; }
        public string clever_output { get; set; }
        public string clever_match { get; set; }
        public string CSRES30 { get; set; }
        public string time_elapsed { get; set; }
        public string filtered_input { get; set; }
        public string filtered_input_other { get; set; }
        public string reaction_degree { get; set; }
        public string emotion_degree { get; set; }
        public string reaction_values { get; set; }
        public string emotion_values { get; set; }
        public string callback { get; set; }
        public string interaction_1 { get; set; }
        public string interaction_1_other { get; set; }
        public string interaction_2 { get; set; }
        public string interaction_3 { get; set; }
        public string interaction_4 { get; set; }
        public string interaction_5 { get; set; }
        public string interaction_6 { get; set; }
        public string interaction_7 { get; set; }
        public string interaction_8 { get; set; }
        public string interaction_9 { get; set; }
        public string interaction_10 { get; set; }
        public string interaction_11 { get; set; }
        public string interaction_12 { get; set; }
        public string interaction_13 { get; set; }
        public string interaction_14 { get; set; }
        public string interaction_15 { get; set; }
        public string interaction_16 { get; set; }
        public string interaction_17 { get; set; }
        public string interaction_18 { get; set; }
        public string interaction_19 { get; set; }
        public string interaction_20 { get; set; }
        public string interaction_21 { get; set; }
        public string interaction_22 { get; set; }
        public string interaction_23 { get; set; }
        public string interaction_24 { get; set; }
        public string interaction_25 { get; set; }
        public string interaction_26 { get; set; }
        public string interaction_27 { get; set; }
        public string interaction_28 { get; set; }
        public string interaction_29 { get; set; }
        public string interaction_30 { get; set; }
        public string interaction_31 { get; set; }
        public string interaction_32 { get; set; }
        public string interaction_33 { get; set; }
        public string interaction_34 { get; set; }
        public string interaction_35 { get; set; }
        public string interaction_36 { get; set; }
        public string interaction_37 { get; set; }
        public string interaction_38 { get; set; }
        public string interaction_39 { get; set; }
        public string interaction_40 { get; set; }
        public string interaction_41 { get; set; }
        public string interaction_42 { get; set; }
        public string interaction_43 { get; set; }
        public string interaction_44 { get; set; }
        public string interaction_45 { get; set; }
        public string interaction_46 { get; set; }
        public string interaction_47 { get; set; }
        public string interaction_48 { get; set; }
        public string interaction_49 { get; set; }
        public string interaction_50 { get; set; }
    }


}
    

