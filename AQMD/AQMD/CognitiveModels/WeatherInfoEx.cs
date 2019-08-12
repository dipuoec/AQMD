// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;

namespace AQMD.CognitiveModels
{
    // Extends the partial FlightBooking class with methods and properties that simplify accessing entities in the luis results
    public partial class WeatherInfo
    {
        public string Location
        {
            get
            {
                var location = Entities?._instance?.Location?.FirstOrDefault()?.Text;
                return location;
            }
        }

        public string Date
        {
            get
            {
                var date = Entities?._instance?.Date?.FirstOrDefault()?.Text;
                return date;
            }
        }
    }
}
