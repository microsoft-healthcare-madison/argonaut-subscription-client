using System;
using System.Collections.Generic;
using System.Text;

namespace argonaut_subscription_client_host.Models
{
    public class TriggerRequest
    {
        #region Class Variables . . .

        #endregion Class Variables . . .

        #region Instance Variables . . .

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets URL of the FHIR server.</summary>
        ///
        /// <value>The FHIR server URL.</value>
        ///-------------------------------------------------------------------------------------------------

        public string FhirServerUrl { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the name of the main resource this trigger will generate.</summary>
        ///
        /// <value>The name of the resource.</value>
        ///-------------------------------------------------------------------------------------------------

        public string ResourceName { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the name of the filter.</summary>
        ///
        /// <value>The name of the filter.</value>
        ///-------------------------------------------------------------------------------------------------

        public string FilterName { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the type of the filter match.</summary>
        ///
        /// <value>The type of the filter match.</value>
        ///-------------------------------------------------------------------------------------------------

        public string FilterMatchType { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the filter value.</summary>
        ///
        /// <value>The filter value.</value>
        ///-------------------------------------------------------------------------------------------------

        public string FilterValue { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the number of repetitions to attempt.</summary>
        ///
        /// <value>The repetitions.</value>
        ///-------------------------------------------------------------------------------------------------

        public int? Repetitions { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the delay milliseconds.</summary>
        ///
        /// <value>The delay milliseconds.</value>
        ///-------------------------------------------------------------------------------------------------

        public int? DelayMilliseconds { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets a value indicating whether the processor should ignore errors.</summary>
        ///
        /// <value>True to ignore errors, false to not.</value>
        ///-------------------------------------------------------------------------------------------------

        public bool? IgnoreErrors { get; set; }

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
