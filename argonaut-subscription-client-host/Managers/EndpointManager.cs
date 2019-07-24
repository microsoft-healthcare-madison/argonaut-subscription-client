using argonaut_subscription_client_host.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace argonaut_subscription_client_host.Managers
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>Manager singleton for endpoints.</summary>
    ///
    /// <remarks>Gino Canessa, 7/18/2019.</remarks>
    ///-------------------------------------------------------------------------------------------------

    public class EndpointManager
    {
        #region Class Variables . . .

        /// <summary>The instance for singleton pattern.</summary>
        private static EndpointManager _instance;

        #endregion Class Variables . . .

        #region Instance Variables . . .

        /// <summary>Dictionary of endpoints, by uid.</summary>
        private Dictionary<Guid, EndpointInformation> _uidEndpointDict;

        #endregion Instance Variables . . .

        #region Constructors . . .

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Constructor that prevents any external instance of this class from being created.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///-------------------------------------------------------------------------------------------------

        private EndpointManager()
        {
            // **** create our index objects ****

            _uidEndpointDict = new Dictionary<Guid, EndpointInformation>();
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
        #endregion Class Interface . . .

        #region Instance Interface . . .

        #endregion Instance Interface . . .

        #region Internal Functions . . .

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Check or create instance.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///-------------------------------------------------------------------------------------------------

        private static void CheckOrCreateInstance()
        {
            if (_instance == null)
            {
                _instance = new EndpointManager();
            }
        }
        #endregion Internal Functions . . .

    }
}
