using System.Linq;
using CharacterCustomizationTool.Editor.Character;
using UnityEngine;

namespace CharacterCustomizationTool.Editor.Randomizer.Steps.Impl
{
    public class HatsWithHairStep : SlotStepBase, IRandomizerStep
    {
        public override GroupType GroupType => GroupType.HatsWithHair;

        protected override GroupType[] CompatibleGroups => new[]
        {
            GroupType.FaceAccessories,
            GroupType.Shoes,
            GroupType.Gloves,
            GroupType.Ears,
        };

        public override StepResult Process(int count, GroupType[] groups, CustomizableCharacter character)
        {
            var cannotProcess = !groups.Contains(GroupType);
            groups = RemoveSelf(groups);

            if (cannotProcess || Random.value > Probability)
            {
                return new StepResult(0, false, groups);
            }

            var newGroups = groups.Where(g => CompatibleGroups.Contains(g)).ToArray();
            var finalGroups = newGroups.ToList();
            finalGroups.Add(GroupType.HairWithHats);

            return new StepResult(Random.Range(0, count), true, finalGroups.ToArray());
        }
    }
}