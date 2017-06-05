﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyKho.Models.ModelManager;
using QuanLyKho.Models.ModelDB;
using System.Web.Security;
using System.Net;
using QuanLyKho.Models.ViewModel;
using QuanLyKho.Security;

namespace QuanLyKho.Controllers
{

    public class LoginController : Controller
    {
        // GET: Login
        public static ManagerLogin manager = new ManagerLogin();
        [Authorize]
        public ActionResult ProfileUser()
        {
            int id = ManagerLogin.GetId(User.Identity.Name);
            UserProfile profile = manager.GetUserProfile(id);

            if (profile == null)
            {
                return HttpNotFound();
            }

            return View(profile);
        }

        [HttpPost]
        public ActionResult ProfileUser(UserProfile profile)
        {
            using (var db = new QuanLyKhoEntities())
            {


                int? id = db.Table_User.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().UserId;
                if (id != null)
                {
                    var user = db.UserProfiles.Find(id);
                    user.Address = profile.Address;
                    user.Name = profile.Name;
                    user.Sex = profile.Sex;
                  
                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    return HttpNotFound();
                }


            }
            return RedirectToAction("Index", "Hang_Hoa");
        }


       
        [HttpGet]
        public ActionResult LoginUser()
        {

            return View();
        }
        #region Đăng nhập người dùng

        [HttpPost]
     
        public ActionResult LoginUser(Table_User _userName)
        {
            if (ModelState.IsValid)
            {

                if (manager.IsUserName(_userName.UserName))
                {
                    var pass = manager.GetPassword(_userName.UserName);
                    if (_userName.UserPassword == pass)
                    {
                        FormsAuthentication.SetAuthCookie(_userName.UserName, false);

                        int idUser = ManagerLogin.GetId(_userName.UserName);
                        using (var db = new QuanLyKhoEntities())
                        {
                            Session["name"] = db.UserProfiles.Find(idUser).Name;
                        }

                        return RedirectToAction("Index", "Kho_Chua");
                    }
                    else
                    {
                       
                        ModelState.AddModelError("ErrDN", "Mật khẩu chưa đúng");

                    }

                }
                else
                {
                    ModelState.AddModelError("ErrDN", "Không tồn tại tài khoản này");

                }


            }
            return View("LoginUser",_userName);

        }
        #endregion

        #region Đăng xuất

        [Authorize]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("LoginUser", "Login");
        }
        #endregion

        #region Đăng ký tài khoản


        [HttpPost]
        public ActionResult SignUp(Table_User _userName)
        {
            if (ModelState.IsValid)
            {
                ManagerLogin manager = new ManagerLogin();
                manager.AddUser(_userName);
                return RedirectToAction("Index", "HangHoa");
            }
            else
                return View();
        }
        #endregion

        //chua apply 
        #region Kiem tra ton tai tai khoan khi dang ky
        [HttpGet]
        public JsonResult IsExistUser(string userName)
        {
            ManagerLogin manager = new ManagerLogin();
            var ktra = manager.IsExitsUserToSignUp(userName);
            return Json(!ktra, JsonRequestBehavior.AllowGet);
        }


        #endregion 


        public ActionResult UnAuthorize()
        {
            return View();
        }


        [AuthorizeRole(new string[] { "Admin" })]
        public ActionResult CreateUserByAdmin()
        {

            using (var db = new QuanLyKhoEntities())
            {
                //ViewBag.Role = new SelectList(db.Table_Role.ToList(), "RoleId", "RoleName","RoleId");
                ViewBag.Role = db.Table_Role.ToList();

            }


            return View();
        }
        [HttpPost]
        public ActionResult CreateUserByAdmin(User _user)
        {

            using (var db = new QuanLyKhoEntities())
            {
                ViewBag.Role = db.Table_Role.ToList();
                ManagerLogin managerLogin = new ManagerLogin();
                if (ModelState.IsValid)
                {


                    if (managerLogin.IsExitsUserToSignUp(_user.UserName))
                    {
                        Response.Write("<script>alert('Đã tồn tại tài khoản')</script>");
                    }
                    else
                    {
                       

                            try
                            {

                                var tbl_user = new Table_User();
                                tbl_user.UserName = _user.UserName;
                                tbl_user.UserPassword = _user.UserPassword;
                                db.Table_User.Add(tbl_user);
                                db.SaveChanges();
                                var userRole = new UserRole();
                                userRole.UserId = tbl_user.UserId;
                                userRole.RoleId = _user.Role;
                                userRole.IsActive = true;
                                db.UserRoles.Add(userRole);
                                db.SaveChanges();
                                var userProfile = new UserProfile();
                                userProfile.UserId = tbl_user.UserId;
                                userProfile.Name = _user.Name;//
                                db.UserProfiles.Add(userProfile);
                                db.SaveChanges();         
                               

                            }
                            catch
                            {
                                Response.Write("<script>alert('Lỗi submit')</script>");
                               
                            }
                        }




                    }


                }
                return View();
            }

        #region Danh sach người dùng
        public ActionResult ListUser()
        {
            
            return View(ManagerLogin.GetListUser());
        }
        #endregion
        [Authorize]
        public ActionResult ChanUser(string UserName,int Role)
        {
            int id = ManagerLogin.GetId(UserName);
            using (QuanLyKhoEntities db = new QuanLyKhoEntities())
            {
                var tk = db.Table_User.Find(id);
                
                 if(tk.IsActive==true)
                {
                    tk.IsActive = false;
                }
                else if(tk.IsActive==false)
                {
                    tk.IsActive = true;
                }
                db.Entry(tk).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ListUser");
            }


        }

    }
    }
