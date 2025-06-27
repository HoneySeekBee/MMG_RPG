namespace CharacterCustomizationTool.Editor.Randomizer.Steps.Impl
{
    public class PantsStep : SlotStepBase, IRandomizerStep
    {
        public override GroupType GroupType => GroupType.Pants;

        protected override GroupType[] CompatibleGroups => new[]
        {
            GroupType.HatSingle,
            GroupType.Hat,
            GroupType.HatsWithHair,
            GroupType.FaceAccessories,
            GroupType.Glasses,
            GroupType.Shoes,
            GroupType.Hairstyle,
            GroupType.Gloves,
            GroupType.Ears,
        };
    }
}