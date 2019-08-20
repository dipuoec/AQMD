namespace AQMD.Shared
{ 
    using System;
    using System.Threading.Tasks;
    using AdaptiveCards;
    using APIXULib;
    using System.Configuration;
    using System.Collections.Generic;

    [Serializable]
    
    public class GetWeatherInPlaceAction 
    {
        public  Task<object> FulfillAsync(string Place , string Date , string AppKey )
        {
            var result = GetCard(Place , Date , AppKey);
            return Task.FromResult((object)result);
        }
        public Task<object> FulfillAsyncAlexa(string Place, string Date, string AppKey)
        {
            var result = GetAlexaCard(Place, Date , AppKey);
            return Task.FromResult((object)result);
        }

        private static AlexaCardContent GetAlexaCard(string place , string date ,  string AppKey)
        {
            WeatherModel model = new Repository().GetWeatherData(AppKey, GetBy.CityName, place);

            var card = new AlexaCardContent();
            if (model != null)
            {
                if (model.current != null)
                {
                    card.title = $"Current Weather";
                    //card.smallImageUrl = GetIconUrl(model.current.condition.icon);
                    card.text = $"Today the temperature is {model.current.temp_f} farenheit at  { model.location.name} .Winds are {model.current.wind_mph} miles per hour from the {model.current.wind_dir}";
                    return card;
                }

            }
            return null;
        }
        private static AdaptiveCard GetCard(string place, string date , string AppKey)
        {
            //WeatherModel model = new Repository().GetWeatherData(AppKey, GetBy.CityName, place, Days.Five);
            AirQuality model = new AirQuality()
            { AreaNumber = 1, AQI = 32, AQICategory = "Good", ForecastDateTime = DateTime.Now, AreaName = "California", Pollutant = " Ozone (o3)", Ozone = 1.0, PMTen = 1.0, PMTwoFive = 2.5 };
            var card = new AdaptiveCard("1.1");
            if (model != null)
            {   
                    //card.Speak = $"<s>Today the temperature is {model.current.temp_f}</s><s>Winds are {model.current.wind_mph} miles per hour from the {model.current.wind_dir}</s>";
                    AddCurrentWeather(model, card);
                    //AddForecast(place, model, card);
                    return card;
            }
            return null;
        }
        private static void AddCurrentWeather(AirQuality model, AdaptiveCard card)
        {
            var headerContainer = new AdaptiveContainer();
            var header = new AdaptiveColumnSet();
            card.Body.Add(headerContainer);
            var headerColumn = new AdaptiveColumn();
            var textHeader = new AdaptiveTextBlock();
            textHeader.Size = AdaptiveTextSize.Medium;
            textHeader.Weight = AdaptiveTextWeight.Bolder;
            textHeader.Text = "AQMD Air Quality";
            headerColumn.Width = AdaptiveColumnWidth.Stretch;
            headerColumn.Items.Add(textHeader);
            header.Columns.Add(headerColumn);
            headerContainer.Bleed = true;
            headerContainer.Style = AdaptiveContainerStyle.Default;
            headerContainer.Items.Add(header);

            var bodyContainer = new AdaptiveContainer();
            var data = new AdaptiveFactSet();
            data.Spacing = AdaptiveSpacing.ExtraLarge;
            data.Facts.Add(new AdaptiveFact() { Title = "Area Number", Value = model.AreaNumber.ToString() });
            data.Facts.Add(new AdaptiveFact() { Title = "Area Name", Value = model.AreaName.ToString() });
            data.Facts.Add(new AdaptiveFact() { Title = "AQI  Value", Value = model.AQI.ToString() });
            data.Facts.Add(new AdaptiveFact() { Title = "Reading Date", Value = model.ForecastDateTime.ToString() });
            data.Facts.Add(new AdaptiveFact() { Title = "AQI Category", Value = model.AQICategory.ToString() });
            data.Facts.Add(new AdaptiveFact() { Title = "Pollutants", Value = model.Pollutant.ToString() });
            bodyContainer.Items.Add(data);
            card.Body.Add(bodyContainer);
            var detailContainer = new AdaptiveContainer();
            detailContainer.Id = "details1";
            var info = new AdaptiveFactSet();
            info.Spacing = AdaptiveSpacing.ExtraLarge;
            info.Facts.Add(new AdaptiveFact() { Title = "Ozone", Value = model.Ozone.ToString() });
            info.Facts.Add(new AdaptiveFact() { Title = "PM 10", Value = model.PMTen.ToString() });
            info.Facts.Add(new AdaptiveFact() { Title = "PM 2.5", Value = model.PMTwoFive.ToString() });
            detailContainer.Items.Add(info);
            card.Body.Add(detailContainer);
            // body.Facts.Add(new AdaptiveFact() { Title = "Area Name", Value = model.AreaNumber.ToString() });

            //card.Actions.Add(new AdaptiveToggleVisibilityAction { Title = "Show Detail", TargetElements = new List<AdaptiveTargetElement> { "details1" } });
            //var showdetailContainer = new AdaptiveContainer();
            //var showDetailColumnSet = new AdaptiveColumnSet();
            //card.Body.Add(showdetailContainer);
            //var showDetailColumn = new AdaptiveColumn();
            //showDetailColumn.Id = "chevronDown4";
            //showDetailColumn.SelectAction.Type = "Action.ToggleVisibility";
            //showDetailColumn.SelectAction.Title = "show detail";
            //showDetailColumn.SelectAction. = "show detail";
            //var textHeader = new AdaptiveTextBlock();
            //textHeader.Size = AdaptiveTextSize.Medium;
            //textHeader.Weight = AdaptiveTextWeight.Bolder;
            //textHeader.Text = "AQMD Air Quality";
            //headerColumn.Width = AdaptiveColumnWidth.Stretch;
            //headerColumn.Items.Add(textHeader);
            //header.Columns.Add(headerColumn);
            //headerContainer.Bleed = true;
            //headerContainer.Style = AdaptiveContainerStyle.Default;
            //headerContainer.Items.Add(header);

            //card.Body.Add(headerContainer);

            //var headerColumn = new AdaptiveColumn();
            //var textHeader = new AdaptiveTextBlock();
            //textHeader.Size = AdaptiveTextSize.Medium;
            //textHeader.Weight = AdaptiveTextWeight.Bolder;
            //textHeader.Text = "AQMD Air Quality";
            //headerColumn.Width = AdaptiveColumnWidth.Auto;
            //headerColumn.Items.Add(textHeader);
            //header.Columns.Add(headerColumn);
            //headerContainer.Items.Add(header);

            //var currentContainer = new AdaptiveContainer();
            //currentContainer.Style = AdaptiveContainerStyle.Emphasis;
            //var current = new AdaptiveColumnSet();


            //card.Body.Add(currentContainer);





            //var currentColumn2 = new Column();
            //current.Columns.Add(currentColumn2);
            //currentColumn2.Size = "65";

            //string date = DateTime.Parse(model.current.last_updated).DayOfWeek.ToString();

            //AddTextBlock(currentColumn2, $"{model.location.name} ({date})", TextSize.Large, false);
            //AddTextBlock(currentColumn2, $"{model.current.temp_f.ToString().Split('.')[0]}° F", TextSize.Large);
            //AddTextBlock(currentColumn2, $"{model.current.condition.text}", TextSize.Medium);
            //AddTextBlock(currentColumn2, $"Winds {model.current.wind_mph} mph {model.current.wind_dir}", TextSize.Medium);
        }
        //private static void AddForecast(string place, WeatherModel model, AdaptiveCard card)
        //{
        //    var forecast = new ColumnSet();
        //    card.Body.Add(forecast);

        //    foreach (var day in model.forecast.forecastday)
        //    {
        //        if (DateTime.Parse(day.date).DayOfWeek != DateTime.Parse(model.current.last_updated).DayOfWeek)
        //        {
        //            var column = new Column();
        //            AddForcastColumn(forecast, column, place);
        //            AddTextBlock(column, DateTimeOffset.Parse(day.date).DayOfWeek.ToString().Substring(0, 3), TextSize.Medium);
        //            AddImageColumn(day, column);
        //            AddTextBlock(column, $"{day.day.mintemp_f.ToString().Split('.')[0]}/{day.day.maxtemp_f.ToString().Split('.')[0]}", TextSize.Medium);
        //        }
        //    }
        //}
        //private static void AddImageColumn(Forecastday day, Column column)
        //{
        //    var image = new Image();
        //    image.Size = ImageSize.Auto;
        //    image.Url = GetIconUrl(day.day.condition.icon);
        //    column.Items.Add(image);
        //}
        //private static string GetIconUrl(string url)
        //{
        //    if (string.IsNullOrEmpty(url))
        //        return string.Empty;

        //    if (url.StartsWith("http"))
        //        return url;
        //    //some clients do not accept \\
        //    return "https:" + url;
        //}
        //private static void AddForcastColumn(ColumnSet forecast, Column column, string place)
        //{
        //    forecast.Columns.Add(column);
        //    column.Size = "20";
        //    var action = new OpenUrlAction();
        //    action.Url = $"https://www.bing.com/search?q=forecast in {place}";
        //    column.SelectAction = action;
        //}

        //private static void AddTextBlock(Column column, string text, TextSize size, bool isSubTitle = true)
        //{
        //    column.Items.Add(new TextBlock()
        //    {
        //        Text = text,
        //        Size = size,
        //        HorizontalAlignment = HorizontalAlignment.Center,
        //        IsSubtle = isSubTitle,
        //        Separation = SeparationStyle.None
        //    });
        //}

    }
}
