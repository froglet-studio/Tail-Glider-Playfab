using UnityEngine;
using System.Collections;

namespace StarWriter.Utility.Email
{
    public class ShareByEmail //: MonoBehaviour
    {
        public string subject = ""; //Subject line of email
        public string text = "";    //Content of email
        public string recipient = "support@frogletgames.zendesk.com"; // Recipient of email's addresses 

        public ShareByEmail(string subject, string Content, string recipient) 
        { 
            this.subject = subject;
            this.text = Content;
            this.recipient = recipient;
        }

        public void SendEmail()
        {
            NativeShare nativeShare = new NativeShare();

            // Set email
            nativeShare.AddEmailRecipient(recipient);
            nativeShare.SetSubject(subject);
            nativeShare.SetText(text);
            nativeShare.SetCallback(HelpEmailCallback);
            // Share the email
            nativeShare.Share();
        }

        public void SendEmailwithAttachment(string attachmentPath)
        {

            NativeShare nativeShare = new NativeShare();

            // Set email
            nativeShare.AddEmailRecipient(recipient);
            nativeShare.SetSubject(subject);
            nativeShare.SetText(text);
            nativeShare.AddFile(attachmentPath);
            nativeShare.SetCallback(HelpEmailCallback);
            // Share the email
            nativeShare.Share();
        }

        void HelpEmailCallback(NativeShare.ShareResult result, string shareTarget)
        {
            Debug.Log("Send Email - Result: " + result.ToString());
            Debug.Log("Send Email - shareTarget: " + shareTarget);

            // TODO Give the player a thumbs up if the result was successful
        }
    }
}



