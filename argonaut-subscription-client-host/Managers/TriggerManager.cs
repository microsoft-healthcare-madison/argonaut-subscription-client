using argonaut_subscription_client_host.Generators;
using argonaut_subscription_client_host.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace argonaut_subscription_client_host.Managers
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>Manager singleton for triggers.</summary>
    ///
    /// <remarks>Gino Canessa, 8/1/2019.</remarks>
    ///-------------------------------------------------------------------------------------------------

    public class TriggerManager
    {
        #region Class Constants . . .

        /// <summary>The default request timeout milliseconds (one minute).</summary>
        private const long _defaultRequestTimeoutMS = 1000 * 60 * 1;

        #endregion Class Constants . . .

        #region Class Variables . . .

        /// <summary>The instance for singleton pattern.</summary>
        private static TriggerManager _instance;

        #endregion Class Variables . . .

        #region Instance Variables . . .

        private Dictionary<Guid, TriggerInformation> _uidTriggerDict;

        /// <summary>The available resources.</summary>
        private HashSet<string> _availableResources;

        /// <summary>List of removals.</summary>
        private SortedList<DateTime, Guid> _removalList;

        /// <summary>The validated fhir servers.</summary>
        private Dictionary<string, FhirClient> _urlFhirServerDict;

        #endregion Instance Variables . . .

        #region Constructors . . .

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Constructor that prevents a default instance of this class from being created.</summary>
        ///
        /// <remarks>Gino Canessa, 8/1/2019.</remarks>
        ///-------------------------------------------------------------------------------------------------

        private TriggerManager()
        {
            // **** create our hashset of resources we can create ****
            // **** NOTE: these MUST be handled in ProcessTriggerRequest ****

            _availableResources = new HashSet<string>()
            {
                //"Patient",
                "Encounter"
            };

            // **** crate our tracking dictionaries ****

            _uidTriggerDict = new Dictionary<Guid, TriggerInformation>();
            _removalList = new SortedList<DateTime, Guid>();
            _urlFhirServerDict = new Dictionary<string, FhirClient>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Static constructor.</summary>
        ///
        /// <remarks>Gino Canessa, 8/1/2019.</remarks>
        ///-------------------------------------------------------------------------------------------------

        static TriggerManager()
        {

        }

        #endregion Constructors . . .

        #region Class Interface . . .

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Initializes this object.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///-------------------------------------------------------------------------------------------------

        public static void Init()
        {
            // **** make an instance ****

            CheckOrCreateInstance();
        }

        public static bool TryAddRequest(TriggerRequest request, out TriggerInformation info)
        {
            return _instance._TryAddRequest(request, out info);
        }

        #endregion Class Interface . . .

        #region Instance Interface . . .

        #endregion Instance Interface . . .

        #region Internal Functions . . .

        private bool _TryAddRequest(TriggerRequest request, out TriggerInformation info)
        {
            // **** start with no info record ****

            info = null;

            // **** sanity checks ****

            if ((request == null) ||
                (string.IsNullOrEmpty(request.FhirServerUrl)) ||
                (string.IsNullOrEmpty(request.ResourceName)))
            {
                // **** write to console ****

                Console.WriteLine("TriggerManager.TryAddRequest <<<" +
                    " invalid request, missing FHIR Server URL or ResourceName");

                // **** cannot process ****

                return false;
            }

            // **** check for resources we know how to handle ****

            if (!_availableResources.Contains(request.ResourceName))
            {
                // **** write to console ****

                Console.WriteLine($"TriggerManager.TryAddRequest <<<" +
                    $" request for resource cannot be fulfilled: {request.ResourceName}");

                // **** cannot process ****

                return false;
            }

            // **** create our information object ****

            info = new TriggerInformation()
            {
                Uid = Guid.NewGuid(),
                Request = request,
                SentPrimaryObjects = new Bundle(),
                FailedPrimaryObjects = new Bundle(),
                SentSupportingObjects = new Bundle(),
                FailedSupportingObjects = new Bundle(),
                Status = TriggerInformation.TriggerStatuses.Queued,
            };

            // **** determine the time this trigger should be removed ****

            long requestMS = (((request.DelayMilliseconds == null) ? 0 : (int)request.DelayMilliseconds) *
                ((request.Repetitions == null) ? 1 : (int)request.Repetitions));

            // **** make this record available until the allowed time past processing time ****

            info.AvailableUntil = DateTime.Now.AddMilliseconds(requestMS + _defaultRequestTimeoutMS);
            
            // **** insert into our tracking queue ****

            _uidTriggerDict.Add(info.Uid, info);

            // **** insert into our removal list at the desired time ****

            _removalList.Add(info.AvailableUntil, info.Uid);

            // **** start by processing a request (will add to queue if additional processing is required) ****

            ProcessTriggerRequest(ref info);

            // **** success ****

            return true;
        }

        private void ProcessTriggerRequest(ref TriggerInformation triggerInfo)
        {
            // **** get a FHIR Client for this server ****

            if (!TryGetFhirClient(triggerInfo.Request.FhirServerUrl, out FhirClient client))
            {
                // **** fail this request ****

                triggerInfo.Status = TriggerInformation.TriggerStatuses.Error;

                // **** nothing else to do ****

                return;
            }

            bool sent;

            // **** handle known types ****

            switch (triggerInfo.Request.ResourceName)
            {
                case "Patient":
                    sent = false;
                    break;

                case "Encounter":

                    sent = SendEncounter(ref triggerInfo, client);

                    break;

                default:
                    // **** fail ****

                    sent = false;

                    break;
            }

            if (!sent)
            {
                // **** fail this request ****

                triggerInfo.Status = TriggerInformation.TriggerStatuses.Error;

                // **** nothing else to do ****

                return;
            }

            // **** check if we need to insert into the processing queue ****

        }

        private bool SendEncounter(ref TriggerInformation triggerInfo, FhirClient client)
        {
            // **** right now the only valid match is a Patient ****

            if (triggerInfo.Request.FilterName != "patient")
            {
                // **** output to screen ****

                Console.WriteLine($"TriggerManager.SendEncounter <<< invalid FilterName: {triggerInfo.Request.FilterName}");

                // **** fail ****

                return false;
            }

            Patient patient = null;

            // **** check for patient equality ****

            if (triggerInfo.Request.FilterMatchType.Equals("="))
            {
                //Bundle results = client.SearchById<Patient>(triggerInfo.Request.FilterValue);

                //if ((results.Entry != null) && (results.Entry.Count > 0))
                //{
                //    patient = results.Entry[0].Resource as Patient;
                //}

                try
                {
                    // **** check to see if the server does not have a matching patient ****

                    patient = client.Read<Patient>(triggerInfo.Request.FilterValue);
                }
                catch (Exception) { /* ignore */ }

                if (patient == null)
                {
                    // **** create a patient record ****

                    PatientGenerator.Generate(out patient);

                    // **** set the ID we need ***

                    patient.Id = IdFromReference(triggerInfo.Request.FilterValue);

                    // **** send patient record to server ****

                    try
                    {
                        patient = client.Update<Patient>(patient);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"TriggerManager.SendEncounter <<< could not create patient: {ex.Message}");
                        return false;
                    }
                }
            }
            else
            {
                // **** output to screen ****

                Console.WriteLine($"TriggerManager.SendEncounter <<< invalid FilterMatchType: {triggerInfo.Request.FilterMatchType}");

                // **** fail ****

                return false;
            }

            // **** create an encounter resource for this patient ****

            EncounterGenerator.Generate(
                $"Patient/{patient.Id}",
                out Encounter encounter
                );

            // **** send the encounter to the server ****

            try
            {
                client.Create<Encounter>(encounter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TriggerManager.SendEncounter <<< could not create encounter: {ex.Message}");
                return false;
            }

            // **** done ****

            return true;
        }

        private static string IdFromReference(string reference)
        {
            if (!reference.Contains('/'))
            {
                return reference;
            }

            return reference.Substring(reference.LastIndexOf('/') + 1);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Attempts to get a FhirClient for the given URL.</summary>
        ///
        /// <remarks>Gino Canessa, 8/1/2019.</remarks>
        ///
        /// <param name="url">   URL of the resource.</param>
        /// <param name="client">[out] The client.</param>
        ///
        /// <returns>True if it succeeds, false if it fails.</returns>
        ///-------------------------------------------------------------------------------------------------

        private bool TryGetFhirClient(string url, out FhirClient client)
        {
            string urlLower = url.ToLower();

            // **** check to see if this URL has already been validated ****

            if (_urlFhirServerDict.ContainsKey(urlLower))
            {
                // **** grab this reference ****

                client = _urlFhirServerDict[urlLower];

                // **** this server is valid ****

                return true;
            }

            // **** check for a valid url ****

            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                // **** invalid url ****

                client = null;
                return false;
            }

            // **** attempt to create our client ****

            try
            {
                client = new FhirClient(url)
                {
                    PreferredFormat = ResourceFormat.Json,
                    PreferredReturn = Prefer.ReturnRepresentation,
                };

                // **** add to our client dict ****

                _urlFhirServerDict.Add(urlLower, client);

                // **** valid ****

                return true;
            }
            catch (Exception ex)
            {
                // **** log for reference ****

                Console.WriteLine($"TriggerManager.TryGetFhirClient <<< exception! url: {url}, exception: {ex.Message}");
            }

            // **** still here means failure ****

            client = null;
            return false;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Check or create instance.</summary>
        ///
        /// <remarks>Gino Canessa, 8/1/2019.</remarks>
        ///-------------------------------------------------------------------------------------------------

        private static void CheckOrCreateInstance()
        {
            if (_instance == null)
            {
                _instance = new TriggerManager();
            }
        }
        #endregion Internal Functions . . .

    }
}
