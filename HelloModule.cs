﻿using Venereissu_backend.Business;
using Venereissu_backend.Models;
using Venereissu_backend;
using Nancy;
using Nancy.ModelBinding;
using SimpleHashing.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Venereissu_backend
{
    public class HelloModule : NancyModule
    {




        public HelloModule()
        {
            var db = new VenereissutDataContext();

            Get["/"] = parameters => "Hello World";


            // Login
            Post["/Login"] = p =>
            {
            Login model = this.Bind();
            // Haetaan käyttäjän tiedot tietokannasta username:n perusteella
            User q = (from a in db.Users where model.username == a.UserName select a).FirstOrDefault();
            List<User> everything = (from a in db.Users select a).ToList();
            ISimpleHash simpleHash = new SimpleHash();
            if (simpleHash.Verify(model.passwd, q.Password))
                {
                // Login ok, annetaan sessionId ja tallennetaan se käyttäjälle.
                string sessionId = Util.CreateRandomPassword(20);
                    q.SessionId = sessionId;
                    q.TimeStamp = DateTime.Now;
                    db.SubmitChanges();
                    return sessionId;
                }
                // Login ei ok, ei palauteta mitään.
                return String.Empty;
            };

            // Logoff

            Post["/Logoff"] = p =>
            {
                return "Logoff OK.";
            };

            Post["/addUser"] = p =>
            {
                Login model = this.Bind();               
                ISimpleHash simpleHash = new SimpleHash();               
                string saltedPasswd = simpleHash.Compute(model.passwd);
                User user = new User { UserName = model.username, Password = saltedPasswd };            
                db.Users.InsertOnSubmit(user);
                db.SubmitChanges();

                return "Operation successful.";
            };


            //Post["/addKohde"] = p =>
            //{
            //    Kohteet model = this.Bind();              
             
            //    db.Kohteets.InsertOnSubmit(model);
            //    db.SubmitChanges();
            //    return "Done inserting Kohde!";
            //};


            Post["/addKohde"] = p =>
            {
                  KohdeWAuthentication m = this.Bind();
                //Kohde km = this.Bind();
                
                if (!Authenticate(m.token, db)) return String.Empty;
                Kohteet k = new Kohteet { Kohde_Id = m.Kohde_Id, Koordinaatit = m.Koordinaatit, KuvaBase64 = m.KuvaBase64, Kuvausteksti = m.Kuvausteksti, Nimi = m.Nimi };
                //Kohteet k = new Kohteet { Koordinaatit = km.Koordinaatit, Nimi = km.Nimi };
                db.Kohteets.InsertOnSubmit(k);
                db.SubmitChanges();
                return "Done inserting Kohde!";
            };


            Post["/addKohteenReissut"] = p =>
            {
                KohteetReissut model = this.Bind();
                db.KohteetReissuts.InsertOnSubmit(model);
                db.SubmitChanges();
                return "Done inserting KohteenReissut!";
            };

            //Post["/addReissu"] = p =>
            //{
            //    Reissut model = this.Bind();
            //    db.Reissuts.InsertOnSubmit(model);
            //    db.SubmitChanges();
            //    return "Done inserting Reissuts!";
            //};


            Post["/addReissu"] = p =>
            {
                ReissutWAuthentication model = this.Bind();
                if (!Authenticate(model.token, db)) return String.Empty;
                string userName = GetUserNameByToken(model.token, db);
                Reissut m = new Reissut { UserName = userName, Alkoi = model.Alkoi, Nimi = model.Nimi, Kuvausteksti = model.Kuvausteksti };
                db.Reissuts.InsertOnSubmit(m);
                db.SubmitChanges();
                return m.Reissu_Id.ToString();
            };




            Get["/Kohteet/{id}"] = p => (GetKohde(p.id, db));
        
        


        }

        private string GetUserNameByToken(string token, VenereissutDataContext db)
        {
            var line = (from a in db.Users where a.SessionId == token select a).FirstOrDefault();
            return line.UserName;
        }
     

        private bool Authenticate(string token, VenereissutDataContext db)
        {
            var q = (from a in db.Users where a.SessionId == token select a).FirstOrDefault();
            if (q != null) {
                if (q.TimeStamp != null && q.TimeStamp.Value.AddMinutes(20) >= DateTime.Now)
                { 
                    q.TimeStamp = DateTime.Now;                    
                    db.SubmitChanges();
                    return true;
                }
            }
         return false;
        }

        private dynamic GetKohde(dynamic id, VenereissutDataContext db)
        {
            var idInt = 0;
            bool muunna = Int32.TryParse((string)id, out idInt);
             var q = (from a in db.Kohteets where a.Kohde_Id == idInt select a).FirstOrDefault();
            //var q = (from a in db.Tehtavats select new { TehtavanNimi = a.Tehtava, Tekija = a.Kayttajat.Nimi }).ToList();
            return q;
        }

        public HelloModule(string modulePath) : base(modulePath)
        {
        }
    }
    
    
}
