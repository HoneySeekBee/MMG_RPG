using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMG
{
    public class BattleInput : InputBase<BattleInputData>
    {
        BattleSkill CurrentSkill = BattleSkill.None;
        BattleInputData battleInputData = new BattleInputData();
        private void Start()
        {
            Initialize();
        }
        protected override void Initialize()
        {
            MMG_KeyCodes = new List<MMG_KeyCode>();

            AddKeycode("skill_1", KeyCode.Alpha1);
            AddKeycode("skill_2", KeyCode.Alpha2);
            AddKeycode("skill_3", KeyCode.Alpha3);
            AddKeycode("skill_4", KeyCode.Alpha4);

            AddKeycode("LeftClick", KeyCode.Mouse0);
            AddKeycode("RightClick", KeyCode.Mouse1);

            base.Initialize();
        }
        protected override void CheckInput()
        {

            if (!GameContextManager.Is(GameContext.Battle))
                return;


            if (keys.TryGetValue("skill_1", out var Skill1) && Input.GetKeyDown(Skill1))
            {
                CurrentSkill = BattleSkill.Skill_1;
            }
            if (keys.TryGetValue("skill_2", out var Skill2) && Input.GetKeyDown(Skill2))
            {
                CurrentSkill = BattleSkill.Skill_2;
            }
            if (keys.TryGetValue("skill_3", out var Skill3) && Input.GetKeyDown(Skill3))
            {
                CurrentSkill = BattleSkill.Skill_3;
            }
            if (keys.TryGetValue("skill_4", out var Skill4) && Input.GetKeyDown(Skill4))
            {
                CurrentSkill = BattleSkill.Skill_4;
            }
            battleInputData.skillType = CurrentSkill;

            if (keys.TryGetValue("LeftClick", out var LeftClick) && Input.GetKeyDown(LeftClick))
            {
                battleInputData.isLeftClick = true;
                InvokeAction(battleInputData);
                CurrentSkill = BattleSkill.None;
            }
            if (keys.TryGetValue("RightClick", out var RightClick) && Input.GetKeyDown(RightClick))
            {
                battleInputData.isLeftClick = false;
                InvokeAction(battleInputData);
                CurrentSkill = BattleSkill.None;
            }
        }
    }
}