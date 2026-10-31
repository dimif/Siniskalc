namespace Siniskalc.Homm3.Entities
{
    /// <summary>
    /// The name of each building whose effect a hero keeps (<see cref="VisitedBuildingEffects"/>), as the editor shows it.
    /// </summary>
    public static class VisitedBuildingNames
    {
        /// <summary>
        /// Returns the name of the building of <paramref name="effect"/> (one flag), such as "Watering hole".
        /// </summary>
        public static string Of(VisitedBuildingEffects effect) => effect switch
        {
            VisitedBuildingEffects.Stables => "Stables",
            VisitedBuildingEffects.SwanPond => "Swan pond",
            VisitedBuildingEffects.WateringHole => "Watering hole",
            VisitedBuildingEffects.Crypt => "Crypt",
            VisitedBuildingEffects.FaerieRing => "Faerie ring",
            VisitedBuildingEffects.FountainOfFortune => "Fountain of fortune",
            VisitedBuildingEffects.IdolOfFortune => "Idol of fortune",
            _ => effect.ToString(),
        };
    }
}
