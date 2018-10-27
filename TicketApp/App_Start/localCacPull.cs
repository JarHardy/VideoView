using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using System.Configuration;
using System.Data;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Net;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.ComponentModel;

namespace TicketApp
{

    public class localCacPull : Controller
    {
        //
        // GET: /localCacPull
        public ActionResult Index()
        {
            return View();
        }


        public PersonClass GetCertInfo()
        {
            PersonClass person = new PersonClass();



            #region Get From Cert Store
            try
            {
                X509Store store = new X509Store(StoreLocation.CurrentUser);
                //X509Store store = new X509Store(StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection coll = store.Certificates;
                Debug.WriteLine("Cert Collection Count: {0}", coll.Count.ToString());
               // X509Certificate2Collection scollection = X509Certificate2UI.SelectFromCollection(fcollection, "Test Certificate Select", "Select a certificate from the following list to get information on that certificate", X509SelectionFlag.MultiSelection);
                //for (int i = 0; i < 2; i++)
                //{

                    //X509Certificate2 cert = coll.Find(X509FindType.FindBySubjectDistinguishedName, "CN=HARDY.JARROD.N.1515616965, OU=CONTRACTOR, OU=PKI, OU=DoD, O=U.S. Government, C=US", true)[i];
                    
                    foreach(X509Certificate2 cert in store.Certificates)
                    {
                        byte[] raw = cert.RawData;
                        Debug.WriteLine("Content Type: {0}{1}", X509Certificate2.GetCertContentType(raw), Environment.NewLine);
                        Debug.WriteLine("Friendly Name: {0}{1}", cert.FriendlyName, Environment.NewLine);
                    //    Debug.WriteLine("Certificate Verified?: {0}{1}", cert.Verify(), Environment.NewLine);
                        Debug.WriteLine("Simple Name: {0}{1}", cert.GetNameInfo(X509NameType.SimpleName, true), Environment.NewLine);
                      //  Debug.WriteLine("Signature Algorithm: {0}{1}", cert.SignatureAlgorithm.FriendlyName, Environment.NewLine);
                     //   Debug.WriteLine("Private Key: {0}{1}", cert.PrivateKey.ToXmlString(false), Environment.NewLine);
                      //  Debug.WriteLine("Public Key: {0}{1}", cert.PublicKey.Key.ToXmlString(false), Environment.NewLine);
                      //  Debug.WriteLine("Certificate Archived?: {0}{1}", cert.Archived, Environment.NewLine);
                        Debug.WriteLine("Length of Raw Data: {0}{1}", cert.RawData.Length, Environment.NewLine);
                        //X509Certificate2UI.DisplayCertificate(cert);
                        //cert.Reset();


                    // Get EDI-PI from Cert
                    //string subjectDN = cert.GetNameInfo(X509NameType.SimpleName, false);  // Works with local X.509 Certs
                    //string subjectDN = cert.GetNameInfo(X509NameType.SimpleName, false);
                    //string subjectDN = cert.SubjectName.Decode(X500DistinguishedNameFlags.None);    // Works with local X.509 Certs
                    /*string subjectDN = cert.GetNameInfo(X509NameType.SimpleName, false);    // Works with local X.509 Certs
                    int myint = subjectDN.LastIndexOf(".") + 1;
                    string edipi = subjectDN.Substring(myint, 10);*/

                    string subjectDN = cert.Subject;
                    int myint = subjectDN.IndexOf(",") - 10;
                    string edipi = subjectDN.Substring(myint, 10);



                    Debug.WriteLine("********* Found Certificate *********");
                    Debug.WriteLine("Subject: {0}", subjectDN);
                    person.subjectDN = subjectDN;
                    Debug.WriteLine("EDI-PI: {0}", edipi);
                    person.edipi = edipi;
                    Debug.WriteLine("Friendly Name: {0}", cert.FriendlyName);
                    person.FriendlyName = cert.FriendlyName;
                    //Debug.WriteLine("Effective Date: {0}", cert.GetEffectiveDateString());
                   // person.GetEffectiveDateString = cert.GetEffectiveDateString();
                   // Debug.WriteLine("Expiration Date: {0}", cert.GetExpirationDateString());
                   // person.GetExpirationDateString = cert.GetExpirationDateString();
                    //Debug.WriteLine("Certificate Verified: {0}", cert.Verify());
                   // Debug.WriteLine("Certificate Archived: {0}", cert.Archived.ToString());
                  //  Debug.WriteLine("Key Algorithm: {0}", cert.GetKeyAlgorithm());
                    Debug.WriteLine("SimpleName: {0}", cert.GetNameInfo(X509NameType.SimpleName, false));
                    person.SimpleName = cert.GetNameInfo(X509NameType.SimpleName, false);
                    

                    string name = (string)person.SimpleName.ToString();
                    name = name.Trim(new Char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9' });
                    name = name.Replace(".", ", ");

                    person.SimpleName = name;
                    Debug.WriteLine(person.SimpleName);

                    string Lname = name.Split(',')[0];
                    string Fname = name.Split(',')[1];
                   
                    person.LastName = Lname;
                    person.FirstName = Fname.Trim(' '); ;



                    Debug.WriteLine("UrlName: {0}", cert.GetNameInfo(X509NameType.UrlName, false));
                    person.UrlName = cert.GetNameInfo(X509NameType.UrlName, false);
                    Debug.WriteLine("EmailName: {0}", cert.GetNameInfo(X509NameType.EmailName, false));
                    person.EmailName = cert.GetNameInfo(X509NameType.EmailName, false);

                    //Console.WriteLine("Public Key String: {0}", cert.GetPublicKeyString());
                    //Console.WriteLine();
                    //Console.WriteLine("Raw Cert Data String: {0}", cert.GetRawCertDataString());
                    //Console.WriteLine();
                    //Console.WriteLine("Serial Number: {0}", cert.GetSerialNumberString());
                    //Debug.WriteLine("Has Private Key: {0}", cert.HasPrivateKey.ToString());
                    //Debug.WriteLine("Issuer: {0}", cert.Issuer);
                    //Debug.WriteLine("Issuer Name: {0}", cert.IssuerName.Format(false));
                    ////Console.WriteLine("PK Signature Algorithm: {0}", cert.PrivateKey.SignatureAlgorithm);
                    ////Console.WriteLine("PK Key Exchange Algorithm: {0}", cert.PrivateKey.KeyExchangeAlgorithm);
                    ////Console.WriteLine("Public Key EncodedKeyValue: {0}", cert.PublicKey.EncodedKeyValue.Format(false));
                    //Debug.WriteLine("Serial Number: {0}", cert.SerialNumber);
                    //Debug.WriteLine("Subject: {0}", cert.Subject);
                    //Debug.WriteLine("SubjectName: {0}", cert.SubjectName.Name);
                    //Console.WriteLine("Thumbprint: {0}", cert.Thumbprint);
                    //Console.WriteLine("Cert (String): {0}", cert.ToString(true));
                    //Console.WriteLine();
                    //Debug.WriteLine("Version: {0}", cert.Version.ToString());
                    //Debug.WriteLine();

                    // Write out certificate extension info

                    //foreach (X509Extension extension in cert.Extensions)
                    //{
                    //    Debug.WriteLine(extension.Oid.FriendlyName + "(" + extension.Oid.Value + ")");
                    //    if (extension.Oid.FriendlyName == "Subject Alternative Name")
                    //    {
                    //        //Console.WriteLine("Subject Alternative Name Ext: {0}", extension.Oid.Value.ToString());
                    //        if (extension.Format(false).Contains("Principal Name"))
                    //        {
                    //            String strSAN = string.Empty;
                    //            String strPrincipalName = string.Empty;
                    //            strSAN = extension.Format(false);
                    //            strPrincipalName = strSAN.Substring(strSAN.IndexOf("Principal Name="));
                    //            Debug.WriteLine("SAN Extension - Principal Name: {0}", strPrincipalName);
                    //            //Console.WriteLine("Subject Alternative Name Ext: {0}", extension.Format(true));
                    //            //Console.WriteLine();
                    //        }
                    //    }
                    //}

                    if(person != null)
                        break;

                }
              //}

                store.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

            }
            #endregion

            return person;

        }
    }
}
