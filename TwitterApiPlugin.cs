using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DNWS
{
    class TwitterApiPlugin : TwitterPlugin
    {
        public string[] Test()
        {
            return new string[]
            {
                "Hello,",
                "World!"
            };
        }

        public List<User> GetAllUsers()
        {
            using (var context = new TweetContext())
            {
                try
                {
                    List<User> users = context.Users.Where(b => true).Include(b => b.Following).ToList();
                    return users;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public List<Following> GetFollowing(string name)
        {
            using (var context = new TweetContext())
            {
                try
                {
                    List<User> followings = context.Users.Where(b => b.Name.Equals(name)).Include(b => b.Following).ToList();
                    return followings[0].Following;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public override HTTPResponse GetResponse(HTTPRequest request) //OVERRIDE GetResponse function
        {
            HTTPResponse response = new HTTPResponse(200);
            string username = request.getRequestByKey("user");
            string password = request.getRequestByKey("password");
            string following = request.getRequestByKey("follow");
            string message = request.getRequestByKey("message");
            string[] Choose = request.Filename.Split("?");
            //Determine between users and following
            if (Choose[0] == "users")
            {
                //cases for each method
                if (request.Method == "GET")
                {
                    string json = JsonConvert.SerializeObject(GetAllUsers());
                    response.body = Encoding.UTF8.GetBytes(json);
                }
                if (request.Method == "POST")
                {
                    try
                    {
                        Twitter.AddUser(username, password); 
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    catch (Exception)
                    {
                        response.status = 403;
                        response.body = Encoding.UTF8.GetBytes("403 User already exists");
                    }
                }
                if (request.Method == "DELETE")
                {
                    Twitter twitter = new Twitter(username);
                    try
                    {
                        twitter.Del_User(username); //use Del_User to delete user
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not exists");
                    }
                }
            }
            //Determine between users and following
            if (Choose[0] == "following")
            {
                Twitter twitter = new Twitter(username);
                //cases for each method
                if (request.Method == "GET")
                {
                    string json = JsonConvert.SerializeObject(GetFollowing(username));
                    response.body = Encoding.UTF8.GetBytes(json);
                }
                if (request.Method == "POST")
                {
                    if (Twitter.Check_Username(following))
                    {
                        twitter.AddFollowing(following);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    else
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not exists");
                    }
                }
                if (request.Method == "DELETE")
                {
                    try
                    {
                        twitter.RemoveFollowing(following);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not exists");
                    }
                }
            }
            if (Choose[0] == "tweets")
            {
                Twitter twitter = new Twitter(username);
                if (request.Method == "GET")
                {
                    try
                    {
                        string timeline = request.getRequestByKey("timeline");
                        if (timeline == "following")
                        {
                            string json = JsonConvert.SerializeObject(twitter.GetFollowingTimeline());
                            response.body = Encoding.UTF8.GetBytes(json);
                        }
                        else
                        {
                            string json = JsonConvert.SerializeObject(twitter.GetUserTimeline());
                            response.body = Encoding.UTF8.GetBytes(json);
                        }
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not found");
                    }
                }
                if (request.Method == "POST")
                {
                    try
                    {
                        twitter.PostTweet(message);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not found");
                    }
                }
            }
            response.type = "application/json";
            return response;
        }
    }
}