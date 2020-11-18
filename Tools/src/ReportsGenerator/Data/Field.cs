namespace ReportsGenerator.Data
{
    /// <summary>
    /// Represents a one of possible known row fields.
    /// </summary>
    public enum Field
    {
        /// <summary>
        /// Used in JHopkins files.
        /// US only. Federal Information Processing Standards code that uniquely identifies counties within the USA.
        /// </summary>
        FIPS,
        
        /// <summary>
        /// Used in JHopkins files.
        /// County name. US only.
        /// </summary>
        Admin2,
        
        /// <summary>
        /// Province, state or dependency name.
        /// </summary>
        ProvinceState,

        /// <summary>
        /// Country, region or sovereignty name.
        /// </summary>
        /// <remarks>
        /// For JHopkins files: The names of locations included on the Website correspond with the official designations used by the U.S. Department of State.
        /// </remarks>
        CountryRegion,

        /// <summary>
        /// A day for which data is represented.
        /// </summary>
        LastUpdate,

        /// <summary>
        /// Used in JHopkins files.
        ///
        /// Dot latitude on the dashboard.
        /// All points (except for Australia) shown on the map are based on geographic centroids, and are not representative
        /// of a specific address, building or any location at a spatial scale finer than a province/state.
        /// Australian dots are located at the centroid of the largest city in each state.
        /// </summary>
        Latitude,

        /// <summary>
        /// Used in JHopkins files.
        ///
        /// Dot longitude on the dashboard.
        /// All points (except for Australia) shown on the map are based on geographic centroids, and are not representative
        /// of a specific address, building or any location at a spatial scale finer than a province/state.
        /// Australian dots are located at the centroid of the largest city in each state.
        /// </summary>
        Longitude,

        /// <summary>
        /// A number of total confirmed cases
        /// </summary>
        Confirmed,

        /// <summary>
        /// A number of total deaths cases.
        /// </summary>
        Deaths,

        /// <summary>
        /// A number of total recovered cases.
        /// </summary>
        /// <remarks>
        /// For Jhopkins files:
        ///
        /// Recovered cases are estimates based on local media reports, and state and local reporting when available,
        /// and therefore may be substantially lower than the true number.
        /// US state-level recovered cases are from [COVID Tracking Project](https://covidtracking.com/).
        /// </remarks>
        Recovered,

        /// <summary>
        /// A number of total active cases.
        /// </summary>
        /// <remarks>
        /// This value is ignored in <see cref="Model.Entry"/> since it's inaccurate.
        /// </remarks>
        Active,

        /// <summary>
        /// Information string. Ignored.
        /// </summary>
        CombinedKey,

        /// <summary>
        /// Incidence Rate = cases per 100,000 persons. Ignored.
        /// </summary>
        IncidenceRate,

        /// <summary>
        /// Case-Fatality Ratio (%) = Number recorded deaths / Number cases. Ignored.
        /// </summary>
        CaseFatalityRatio,

        /// <summary>
        /// A number of deaths cases daily.
        /// </summary>
        DeathsByDay,

        /// <summary>
        /// A number of recovered cases daily.
        /// </summary>
        RecoveredByDay,

        /// <summary>
        /// A number of confirmed cases daily.
        /// </summary>
        ConfirmedByDay,

        /// <summary>
        /// Unique Identifier for each row entry. Ignored.
        /// </summary>
        UID,

        /// <summary>
        /// Officially assigned country code identifier.
        /// </summary>
        Iso2,

        /// <summary>
        /// Officially assigned country code identifier.
        /// </summary>
        Iso3,

        /// <summary>
        /// Officially assigned country code identifier.
        /// </summary>
        Code3,

        /// <summary>
        /// Country / province / county population.
        /// </summary>
        Population,

        /// <summary>
        /// Country / province / county continent name.
        /// </summary>
        ContinentName,

        /// <summary>
        /// Country / province / county continent code.
        /// </summary>
        ContinentCode,

        /// <summary>
        /// Latin name.
        /// </summary>
        English,

        /// <summary>
        /// Cyrillic name.
        /// </summary>
        Russian,

        /// <summary>
        /// State abbreviation.
        /// </summary>
        Abbreviation,

        /// <summary>
        /// State name.
        /// </summary>
        Name
    }
}