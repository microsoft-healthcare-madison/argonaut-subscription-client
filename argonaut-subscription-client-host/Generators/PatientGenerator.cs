using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace argonaut_subscription_client_host.Generators
{
    public abstract class PatientGenerator
    {
        #region Class Constants . . .

        private const int _secondsPerYear = 365 * 24 * 60 * 60;
        private const int _secondsPerLeapYear = 366 * 24 * 60 * 60;

        #endregion Class Constants . . .

        #region Class Variables . . .

        private static Random _rand;

        #endregion Class Variables . . .

        #region Instance Variables . . .

        #endregion Instance Variables . . .

        #region Constructors . . .

        static PatientGenerator()
        {
            _rand = new Random();
        }

        #endregion Constructors . . .

        #region Class Interface . . .

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Generates an OBVIOUS test patient.</summary>
        ///
        /// <remarks>Gino Canessa, 8/1/2019.</remarks>
        ///
        /// <param name="patient">[out] The patient.</param>
        ///-------------------------------------------------------------------------------------------------

        public static void Generate(out Patient patient)
        {
            // **** create a birth date ***

            GeneratePatientBirthDate(out DateTime birthDate);

            // **** create a gender ****

            GeneratePatientGender(out AdministrativeGender gender);

            // **** generate an obvious test name ****

            GeneratePatientName(out HumanName name);

            // **** generate our patient ****

            patient = new Patient()
            {
                BirthDate = birthDate.ToString("yyyy-MM-dd"),
                Gender = gender,
                Name = new List<HumanName>() { name }
            };
        }

        #endregion Class Interface . . .

        #region Instance Interface . . .

        #endregion Instance Interface . . .

        #region Internal Functions . . .

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Generates a patient name.</summary>
        ///
        /// <remarks>Gino Canessa, 8/1/2019.</remarks>
        ///
        /// <param name="name">[out] The name.</param>
        ///-------------------------------------------------------------------------------------------------

        private static void GeneratePatientName(out HumanName name)
        {
            name = new HumanName()
            {
                Family = $"Project-{_rand.Next(1000,9999)}",
                Given = new string[] { $"Argonaut-{_rand.Next(1000,9999)}" },
                Use = HumanName.NameUse.Official
            };
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Generates a patient gender.</summary>
        ///
        /// <remarks>Gino Canessa, 8/1/2019.</remarks>
        ///
        /// <param name="patientGender">[out] The patient gender.</param>
        ///-------------------------------------------------------------------------------------------------

        private static void GeneratePatientGender(out AdministrativeGender patientGender)
        {
            // **** default to f/m ****

            patientGender = (_rand.NextDouble() < 0.51) ? AdministrativeGender.Female : AdministrativeGender.Male;

            // **** check for unknown/other ****

            if (_rand.NextDouble() < 0.05)
            {
                patientGender = AdministrativeGender.Other;
            }
            else if (_rand.NextDouble() < 0.05)
            {
                patientGender = AdministrativeGender.Unknown;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Generates a patient birth date.</summary>
        ///
        /// <remarks>Gino Canessa, 8/1/2019.</remarks>
        ///
        /// <param name="birthDateTime">[out] The birth date time.</param>
        ///-------------------------------------------------------------------------------------------------

        private static void GeneratePatientBirthDate(out DateTime birthDateTime)
        {
            // **** generate a birth year ****

            int age = _rand.Next(110);
            int birthYear = DateTime.Today.Year - age;
            int birthSeconds;

            // **** generate a birth day / time ****

            if (DateTime.IsLeapYear(birthYear))
            {
                birthSeconds = _rand.Next(_secondsPerLeapYear);
            }
            else
            {
                birthSeconds = _rand.Next(_secondsPerYear);
            }

            birthDateTime = new DateTime(birthYear, 1, 1);
            birthDateTime = birthDateTime.AddSeconds(birthSeconds);

            // **** any birthdates in the future should be truncated to now ****

            if (birthDateTime > DateTime.Now)
            {
                birthDateTime = DateTime.Now;
            }
        }


        #endregion Internal Functions . . .

    }
}
