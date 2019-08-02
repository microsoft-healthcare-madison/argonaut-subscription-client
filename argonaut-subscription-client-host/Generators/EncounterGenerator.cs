using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace argonaut_subscription_client_host.Generators
{
    public abstract class EncounterGenerator
    {
        #region Class Variables . . .

        #endregion Class Variables . . .

        #region Instance Variables . . .

        #endregion Instance Variables . . .

        #region Constructors . . .

        #endregion Constructors . . .

        #region Class Interface . . .

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Generates an encounter.</summary>
        ///
        /// <remarks>Gino Canessa, 8/1/2019.</remarks>
        ///
        /// <param name="patient">        The patient.</param>
        /// <param name="encounter">      [out] The encounter.</param>
        /// <param name="encounterStatus">(Optional) The encounter status.</param>
        /// <param name="classCode">      (Optional) The class code (AMB|EMER|FLD|HH|IMP|ACUTE|NONAC|OBSENC|PRENC|SS|VR)</param>
        ///-------------------------------------------------------------------------------------------------

        public static void Generate(
                                    string patientRef, 
                                    out Encounter encounter, 
                                    Encounter.EncounterStatus encounterStatus = Encounter.EncounterStatus.InProgress,
                                    string classCode = "VR"
                                    )
        {
            // **** create the most basic encounter record we can think of ***

            encounter = new Encounter()
            {
                Status = encounterStatus,
                Subject = new ResourceReference(patientRef),
                Class = new Coding("http://terminology.hl7.org/CodeSystem/v3-ActCode", classCode)
            };
        }

        #endregion Class Interface . . .

        #region Instance Interface . . .

        #endregion Instance Interface . . .

        #region Internal Functions . . .

        #endregion Internal Functions . . .

    }
}
