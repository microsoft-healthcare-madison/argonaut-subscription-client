using System;
using System.Collections.Generic;
using System.Text;

namespace argonaut_subscription_client_host.Models
{
    public class TriggerInformation
    {
        #region Class Enums . . .

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Values that represent trigger request states.</summary>
        ///
        /// <remarks>Gino Canessa, 8/1/2019.</remarks>
        ///-------------------------------------------------------------------------------------------------

        public enum TriggerStatuses : int
        {
            Unknown = 0,
            Queued,
            Processing,
            Complete,
            Error
        }

        #endregion Class Enums . . .

        #region Class Variables . . .

        #endregion Class Variables . . .

        #region Instance Variables . . .

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the UID.</summary>
        ///
        /// <value>The UID.</value>
        ///-------------------------------------------------------------------------------------------------

        public Guid Uid { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the request.</summary>
        ///
        /// <value>The request.</value>
        ///-------------------------------------------------------------------------------------------------

        public TriggerRequest Request { get; set; }

        public Hl7.Fhir.Model.Bundle SentPrimaryObjects { get; set; }

        public Hl7.Fhir.Model.Bundle FailedPrimaryObjects { get; set; }

        public Hl7.Fhir.Model.Bundle SentSupportingObjects { get; set; }

        public Hl7.Fhir.Model.Bundle FailedSupportingObjects { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the status.</summary>
        ///
        /// <value>The status.</value>
        ///-------------------------------------------------------------------------------------------------

        public TriggerStatuses Status { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the Date/Time when this trigger will be removed.</summary>
        ///
        /// <value>Date/Time when this trigger will be removed.</value>
        ///-------------------------------------------------------------------------------------------------

        public DateTime AvailableUntil { get; set; }

        #endregion Instance Variables . . .

        #region Constructors . . .

        #endregion Constructors . . .

        #region Class Interface . . .

        #endregion Class Interface . . .

        #region Instance Interface . . .

        #endregion Instance Interface . . .

        #region Internal Functions . . .

        #endregion Internal Functions . . .

    }
}
