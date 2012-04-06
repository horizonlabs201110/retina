using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.Imola.Retina.Utility
{
    interface INISensor
    {
    }

    class NISensor : INISensor
    {
        public static INISensor GetInstance()
        {
            if (sensor == null)
            {
                lock (sensorLock)
                {
                    if (sensor == null)
                    {
                        sensor = new OpenNISensor();
                    }
                }
            }
            return sensor;
        }
        
        #region INISensor Member

        #endregion INISensor Memeber

        private static INISensor sensor = null;
        private static object sensorLock = new object();
    }

    class OpenNISensor : INISensor
    {
        public OpenNISensor() 
        { }

        #region INISensor Member


        #endregion INISensor Memeber
    }
}
