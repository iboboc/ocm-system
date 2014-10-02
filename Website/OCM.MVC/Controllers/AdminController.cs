﻿using OCM.API.Common;
using OCM.API.Common.Model;
using OCM.Core.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace OCM.MVC.Controllers
{
    public class AdminController : AsyncController
    {
        //
        // GET: /Admin/
        [AuthSignedInOnly(Roles = "Admin")]
        public ActionResult Index()
        {
            return View();
        }

        [AuthSignedInOnly(Roles = "Admin")]
        public ActionResult Users()
        {
            var userList = new UserManager().GetUsers().OrderByDescending(u => u.DateCreated);
            return View(userList);
        }

        [AuthSignedInOnly(Roles = "Admin")]
        public ActionResult EditUser(int id)
        {
            var user = new UserManager().GetUser(id);
            return View(user);
        }

        [AuthSignedInOnly(Roles = "Admin")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditUser(OCM.API.Common.Model.User userDetails)
        {
            if (ModelState.IsValid)
            {
                var userManager = new UserManager();

                //save
                if (userManager.UpdateUserProfile(userDetails, true))
                {
                    return RedirectToAction("Users");
                }
            }

            return View(userDetails);
        }

        [AuthSignedInOnly(Roles = "Admin")]
        public ActionResult Operators()
        {
            var operatorInfoManager = new OperatorInfoManager();

            return View(operatorInfoManager.GetOperators());
        }

        [AuthSignedInOnly(Roles = "Admin")]
        public ActionResult EditOperator(int? id)
        {
            var operatorInfo = new OperatorInfo();

            if (id != null) operatorInfo = new OperatorInfoManager().GetOperatorInfo((int)id);
            return View(operatorInfo);
        }

        [AuthSignedInOnly(Roles = "Admin")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditOperator(OperatorInfo operatorInfo)
        {
            if (ModelState.IsValid)
            {
                var operatorInfoManager = new OperatorInfoManager();

                operatorInfo = operatorInfoManager.UpdateOperatorInfo(operatorInfo);
                return RedirectToAction("Operators", "Admin");
            }
            return View(operatorInfo);
        }

        [AuthSignedInOnly(Roles = "Admin")]
        public ActionResult CommentDelete(int id)
        {
            var commentManager = new UserCommentManager();
            var user = new UserManager().GetUser(int.Parse(Session["UserID"].ToString()));
            commentManager.DeleteComment(user.ID, id);
            return RedirectToAction("Index");
        }

        [AuthSignedInOnly(Roles = "Admin")]
        public ActionResult MediaDelete(int id)
        {
            var itemManager = new MediaItemManager();
            var user = new UserManager().GetUser(int.Parse(Session["UserID"].ToString()));
            itemManager.DeleteMediaItem(user.ID, id);
            return RedirectToAction("Details", "POI");
        }

        public async Task<JsonResult> PollForTasks(string key)
        {
            int notificationsSent = 0;
            MirrorStatus mirrorStatus = null;
            //poll for periodic tasks (subscription notifications etc)
            if (key == System.Configuration.ConfigurationManager.AppSettings["AdminPollingAPIKey"])
            {
                //send all pending subscription notifications
                try
                {
                    notificationsSent = new UserSubscriptionManager().SendAllPendingSubscriptionNotifications();
                }
                catch (Exception)
                {
                    ; ; //failed to send notifications
                }


                //update cache mirror
                try
                {
                    HttpContext.Application["_MirrorRefreshInProgress"] = true;
                    mirrorStatus = await new CacheProviderMongoDB().PopulatePOIMirror(CacheProviderMongoDB.CacheUpdateStrategy.Modified);
                }
                catch (Exception)
                {
                    ; ;//failed to update cache
                }
                finally {
                    HttpContext.Application["_MirrorRefreshInProgress"] = false;
                }
            }
            return Json(new { NotificationsSent = notificationsSent, MirrorStatus= mirrorStatus }, JsonRequestBehavior.AllowGet);
        }

        [AuthSignedInOnly(Roles = "Admin")]
        public ActionResult CheckPOIMirrorStatus()
        {
            var mirrorManager = new CacheProviderMongoDB();
            var status = mirrorManager.GetMirrorStatus();
            if (status == null)
            {
                status = new MirrorStatus();
                status.StatusCode = System.Net.HttpStatusCode.NotFound;
                status.Description = "Cache is offline";

            }
            else {
                if (HttpContext.Application["_MirrorRefreshInProgress"] != null && (bool)HttpContext.Application["_MirrorRefreshInProgress"] == true)
                {
                    status.Description += " (Update in progress)";
                }
            }
            return View(status);
        }

        [AuthSignedInOnly(Roles = "Admin")]
        public async Task<JsonResult> RefreshPOIMirror(string mode)
        {
            MirrorStatus status = new MirrorStatus();

            if (HttpContext.Application["_MirrorRefreshInProgress"] == null || (bool)HttpContext.Application["_MirrorRefreshInProgress"]==false)
            {
                HttpContext.Application["_MirrorRefreshInProgress"] = true;
                var mirrorManager = new CacheProviderMongoDB();
                
                try
                {
                    if (mode == "repeat")
                    {
                        status = await mirrorManager.PopulatePOIMirror(CacheProviderMongoDB.CacheUpdateStrategy.Incremental);
                        while (status.NumPOILastUpdated > 0)
                        {
                            System.Diagnostics.Debug.WriteLine("Mirror Update:" + status.LastUpdated + " updated, " + status.TotalPOI + " total");
                            status = await mirrorManager.PopulatePOIMirror(CacheProviderMongoDB.CacheUpdateStrategy.Incremental);
                        }
                    }
                    else
                        if (mode == "all")
                        {
                            status = await mirrorManager.PopulatePOIMirror(CacheProviderMongoDB.CacheUpdateStrategy.All);
                        }
                        else
                        {
                            status = await mirrorManager.PopulatePOIMirror(CacheProviderMongoDB.CacheUpdateStrategy.Modified);
                        }

                }
                catch (Exception exp)
                {
                    status.TotalPOI = 0;
                    status.Description = "Cache update error:"+exp.ToString();
                    status.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                }

                HttpContext.Application["_MirrorRefreshInProgress"] = false;
            }
            else
            {
                status.StatusCode = System.Net.HttpStatusCode.PartialContent;
                status.Description = "Update currently in progress";
            }

            return Json(status, JsonRequestBehavior.AllowGet);
        }

    }
}