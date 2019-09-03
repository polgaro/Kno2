using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Kno2.ApiTestClient.Core;
using Kno2.ApiTestClient.Core.Extensions;
using Kno2.ApiTestClient.Core.Helpers;
using Kno2.ApiTestClient.Core.Resources;
using Kno2.ApiTestClient.Send.Extensions;

namespace Kno2.ApiTestClient.Send
{
    public class Program
    {
        static void Main(string[] args)
        {

            //Message DTO
            MessageDTO message = new MessageDTO
            {
                PatientId = "8675309",
                FirstName = "John",
                LastName = "Smith (emr-client)",
                MiddleName = "X",
                Gender = "M",
                BirthDate = new DateTime(1980, 1, 1).ToShortDateString(),
                ToAddress = "208-922-6826",
                DocumentTitle = "Document title goes here",
                DocumentDescription = "Document description goes here",

                Body = "Body goes here ",
                Comments = "Comments go here",

                FileType = FileType.pdf,
                FileBytes = FileHelpers.GetSampleAttachmentBytes(FileType.pdf),

            };

            Kno2Service service = new Kno2Service();

            service.SendKno2Message(message);

            Console.ReadKey();
            Console.ResetColor();
        }

    }

    public class Kno2ConfigurationDTO
    {
        public Uri BaseUri { get; internal set; }
        public string ClientId { get; internal set; }
        public string ClientSecret { get; internal set; }
        public string AppId { get; internal set; }
        public Uri AuthUri { get; internal set; }
        public string EmrSessionValue { get; internal set; }
        public string FromAddress { get; internal set; }
        public string Subject { get; internal set; }
    }

    public class MessageDTO
    {
        public string PatientId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string BirthDate { get; set; }
        public string ToAddress { get; set; }
        public string MiddleName { get; internal set; }
        public string Comments { get; internal set; }
        public string Body { get; internal set; }
        public FileType FileType { get; internal set; }
        public string DocumentTitle { get; internal set; }
        public string DocumentDescription { get; internal set; }
        public byte[] FileBytes { get; internal set; }
    }

    public class Kno2Service
    {
        #region Fields
        public Uri BaseUri;
        private readonly Uri _messagesUriTemplate;
        private readonly Uri _attachmentsUriTemplate;
        private readonly Uri _messageSendUriTemplate;
        private readonly Uri _documentTypesUriTemplate;
        private readonly Uri _directoryValidateTemplate;
        private readonly Uri _messageReadEventUriTemplate;
        private readonly Uri _attachmentReadUriTemplate;

        public string DirectMessageDomain;
        public string EmrSessionValue;
        private readonly Uri _messageSearchTemplate;
        #endregion

        #region Constructor
        public Kno2Service()
        {
            //try to create a direct address domain by convention
            BaseUri = new Uri(ConfigurationManager.AppSettings["BaseUri"]);
            string directMessageDomain = ConfigurationManager.AppSettings["DirectMessageDomain"];
            if (string.IsNullOrWhiteSpace(directMessageDomain) || directMessageDomain == "your_direct_message_domain")
            {
                string[] segments = BaseUri.Host.Split('.');
                if (segments.Length == 3)
                {
                    DirectMessageDomain = string.Format("{0}.direct.{1}.{2}", segments[0], segments[1], segments[2]);
                }
            }
            else
            {
                DirectMessageDomain = directMessageDomain;
            }

            EmrSessionValue = ConfigurationManager.AppSettings["EmrSessionValue"];

            _messagesUriTemplate = new Uri(ConfigurationManager.AppSettings["MessagesUriTemplate"], UriKind.Relative);
            _attachmentsUriTemplate = new Uri(ConfigurationManager.AppSettings["AttachmentsUriTemplate"], UriKind.Relative);
            _messageSendUriTemplate = new Uri(ConfigurationManager.AppSettings["MessageSendUriTemplate"], UriKind.Relative);
            _messageReadEventUriTemplate = new Uri(ConfigurationManager.AppSettings["MessageReadEventUriTemplate"], UriKind.Relative);
            _documentTypesUriTemplate = new Uri(ConfigurationManager.AppSettings["DocumentTypesUriTemplate"], UriKind.Relative);
            _directoryValidateTemplate = new Uri(ConfigurationManager.AppSettings["DirectoryValidateTemplate"], UriKind.Relative);
            _messageSearchTemplate = new Uri(ConfigurationManager.AppSettings["MessageSearchTemplate"], UriKind.Relative);
            _attachmentReadUriTemplate = new Uri(ConfigurationManager.AppSettings["AttachmentReadUriTemplate"], UriKind.Relative);
        }
        #endregion

        #region Get configuration
        public Kno2ConfigurationDTO GetKno2Configuration()
        {
            Kno2ConfigurationDTO ret = new Kno2ConfigurationDTO
            {
                AppId = ConfigurationManager.AppSettings["AppId"],
                AuthUri = new Uri(ConfigurationManager.AppSettings["AuthUri"], UriKind.Relative),
                BaseUri = new Uri(ConfigurationManager.AppSettings["BaseUri"]),
                ClientId = ConfigurationManager.AppSettings["ClientId"],
                ClientSecret = ConfigurationManager.AppSettings["ClientSecret"],
                EmrSessionValue = ConfigurationManager.AppSettings["EmrSessionValue"],
                FromAddress = ConfigurationManager.AppSettings["FromAddress"],
                Subject = ConfigurationManager.AppSettings["Subject"],
            };

            return ret;
        }
        #endregion

        #region Addresses methods
        public Uri AttachmentsUri(string messageId, string attachmentId = null)
        {
            var uriTemplate = new UriTemplate(_attachmentsUriTemplate.ToString());

            if (string.IsNullOrWhiteSpace(attachmentId))
                attachmentId = " ";

            return UriExtensions.TrimUri(uriTemplate.BindByPosition(BaseUri, messageId, attachmentId));
        }

        public Uri MessagesUri(string messageId = null)
        {
            var uriTemplate = new UriTemplate(_messagesUriTemplate.ToString());

            if (string.IsNullOrWhiteSpace(messageId))
                messageId = " ";

            return UriExtensions.TrimUri(uriTemplate.BindByPosition(BaseUri, messageId));
        }

        public Uri MessageSendUri(string messageId)
        {
            var uriTemplate = new UriTemplate(_messageSendUriTemplate.ToString());

            return uriTemplate.BindByPosition(BaseUri, messageId);
        }


        public Uri MessageReadEventUri(string messageId)
        {
            var uriTemplate = new UriTemplate(_messageReadEventUriTemplate.ToString());

            return uriTemplate.BindByPosition(BaseUri, messageId);
        }

        public Uri DocumentTypesUri()
        {
            return _documentTypesUriTemplate;
        }

        public Uri DirectoryValidate()
        {
            return _directoryValidateTemplate;
        }

        public Uri MessageSearch()
        {
            return _messageSearchTemplate;
        }

        public Uri AttachmentReadUri(string messageId, string attachmentId)
        {
            var uriTemplate = new UriTemplate(_attachmentReadUriTemplate.ToString());

            return uriTemplate.BindByPosition(BaseUri, messageId, attachmentId);
        }
        #endregion

        public HttpClient GetHttpClient(Kno2ConfigurationDTO configurationDTO)
        {
            return HttpClientHelper.CreateHttpClient(baseUri: configurationDTO.BaseUri,
                    defaultAccept: "application/json",
                    clientId: configurationDTO.ClientId,
                    clientSecret: configurationDTO.ClientSecret,
                    appId: configurationDTO.AppId,
                    authUri: configurationDTO.AuthUri,
                    grantType: "client_credentials",
                    emrSessionValue: configurationDTO.EmrSessionValue
                    );
        }

        public void SendKno2Message(MessageDTO message)
        {
            try
            {

                //config DTO
                Kno2ConfigurationDTO configDTO = this.GetKno2Configuration();


                //Get the httpClient that will be used for the transmissions
                HttpClient httpClient = this.GetHttpClient(configDTO);


                //Validate the addresses (probably optional)
                Dictionary<string, bool> addressValidationResults =
                    ApiHelper.ValidateAddresses(httpClient: httpClient,
                        directoryValidateUri: this.DirectoryValidate(),
                        addresses: new[] { message.ToAddress, configDTO.FromAddress });

                // Request the available document types (cacheable)
                IEnumerable<string> documentTypes = ApiHelper.RequestDocumentTypes(httpClient: httpClient,
                    documentTypesUri: this.DocumentTypesUri());


                // Request a message draft id
                var outboundMessage = ApiHelper.RequestMessageDraft(httpClient: httpClient,
                      messageUri: this.MessagesUri()
                    );


                var attachmentIds = new List<string>();
                FileType fileType = message.FileType;

                //get a new file name
                string fileName = FileHelpers.GenerateAttachmentName(fileType);

                //upload the attachment
                var attachment = ApiHelper.UploadAttachment(httpClient: httpClient,
                    attachmentsUri: this.AttachmentsUri(outboundMessage.Id),
                    fileName: fileName,
                    attachment: new AttachmentResource
                    {
                        NativeFileName = fileName,
                        NativeFileBytes = message.FileBytes,
                        DocumentType = documentTypes.First(),
                        AttachmentMeta = new AttachmentMetaResource
                        {
                            DocumentTitle = message.DocumentTitle,
                            DocumentType = documentTypes.First(),
                            DocumentDate = DateTime.UtcNow,
                            DocumentDescription = message.DocumentDescription,
                            Confidentiality = Confidentiality.Normal
                        }
                    }
                    );

                // Request the attachment meta data
                var attachments = new List<AttachmentResource>();
                string id = attachment.Id;

                var metadata = ApiHelper.RequestAttachmentMetadata(httpClient: httpClient,
                    attachmentsUri: this.AttachmentsUri(messageId: outboundMessage.Id, attachmentId: id)
                    );
                attachments.Add(metadata);


                // Send the message (draft)
                outboundMessage.Attachments = attachments;
                outboundMessage.Subject = configDTO.Subject;
                outboundMessage.ToAddress = message.ToAddress;
                outboundMessage.FromAddress = configDTO.FromAddress;
                outboundMessage.Comments = message.Comments;
                outboundMessage.Body = message.Body;

                outboundMessage.Patient = new Patient
                {
                    BirthDate = message.BirthDate,
                    FirstName = message.FirstName,
                    Gender = message.Gender,
                    LastName = message.LastName,
                    MiddleName = message.MiddleName,
                    PatientId = message.PatientId,
                };

                ApiHelper.SendDraft(httpClient: httpClient,
                    messageUri: this.MessagesUri(outboundMessage.Id),
                    messageResource: outboundMessage);


                ApiHelper.SendRelease(httpClient: httpClient,
                    messageSendUri: this.MessageSendUri(outboundMessage.Id),
                    messageResource: outboundMessage
                    );

            }
            catch (AggregateException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var innerException in ex.InnerExceptions)
                {
                    sb.AppendLine(innerException.Message);
                    sb.AppendLine(innerException.StackTrace);
                    innerException.Message.ToConsole();
                    if (innerException.InnerException != null)
                    {
                        sb.AppendLine(innerException.InnerException.Message);
                        sb.AppendLine(innerException.InnerException.StackTrace);
                    }
                }
                throw new Exception(sb.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }

    internal static class UriExtensions
    {
        public static Uri TrimUri(this Uri source)
        {
            return new Uri(source.ToString().Trim());
        }
    }
}
