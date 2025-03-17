using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Data
{
    public class Define
    {
        public enum Scene
        {
            Unknown = 0,
            Login = 1,
            GameScene = 2,
            Loading = 3,
            Lobby = 4,
            ResourceRelease = 5,


            Debug = 999,
        }

        public enum EUnitType
        {
            None = 0,
            Melee = 1,
            Ranged = 2,
            Support = 3,
        }

        public enum EStatType
        {
            NONE = 0,
            STR = 1,
            AGI = 2,
            INT = 3,
        }


        public enum EUNIT_STATE
        {
            IDLE = 0,
            WALK = 1,
            RUN = 2,
            ATTACK = 3,
            DIE = 4,
            DESTROY = 5,
        }

        public enum DECIMALROUND
        {
            None,   //소수 현상태 유지
            RoundUp, //올림
            RoundDown,//내림
            Round,//반올림
        }

        public enum Notation
        {
            None,   //미표기
            Amount, //수량 표기
            IsUnits,//k, m등 많은 수량을 표기할때 사용
            Percent,//퍼센트 표기할때 사용
        }

        public enum EPOPUP_TYPE
        {
            None = 0,
            PopupNetWait = 1,
            PopupConfirm = 2,
            PopupYesNo = 3,
            PopupOption = 4,
            PopupLock = 5,
            PopupUnitStatus = 6,
            PopupTeamSetting = 7,
            PopupCountDown = 8,
            PopupPause = 9,
            PopupPush = 10,
            PopupInGameResult = 11,
        }

        public enum EFONT_TYPE
        {
            ENONE = 0,
            Default = 1,
        }

        public enum StringFileType
        {
            Normal = 0,
            ErrorStr = 1,
            BuildStr = 2,
        }

        public enum ECONTENT_TYPE
        {
            LOBBY,
            MAIN,
            SHOP,
            UPGRADE,
            INGAME,
        }

        public enum EUNIT_DIRECTION
        {
            UP,
            DOWN,
            LEFT,
            RIGHT,
        }

        [Flags]
        public enum EGAME_STATE
        {
            LOADING = 0,
            LOADING_COMPLETE = 1 << 1,
            ENTRY = 1 << 2,
            PLAY = 1 << 3,
            PAUSE = 1 << 4,
            RESULT = 1 << 5,
            COMMANDER = 1 << 6,
            MANAGE = 1 << 7,
            ENTRY_COMPLETE = 1 << 8,
        }

        public enum EVENT_TYPE
        {
            NONE = 0,
            Ball = 1,
            Patient = 2,
        }

        public enum TimeType
        {
            UTC = 0,
            Local = 1,
            ServerUTC = 2,
        }

        public enum ETRIGGER_TYPE
        {
            None,
            WaitingRoom,
            EventSpot,
        }

        public enum AssetLabel
        {
            Default = 0,
            Popup = 1,
            Script = 2,
            Font = 3,
            UI = 4,
            Material = 5,
            Particle = 6,
        }

        public enum Sound
        {
            Master = 0,
            Bgm = 1,
            AnotherBgm = 2,
            Effect = 3,
            Voice = 4,
            Max = 5,
        }

        public enum ESKILL_USE_CHECK
        {
            NONE = 0,
            ME = 1,
            PARTY = 2,
            ENEMY = 3
        }

        public enum ESKILL_TARGET_TYPE
        {
            NONE = 0,
        }

        public enum ESEARCH_TARGET_TYPE
        {
            [InspectorName("가까운 적")]
            NeareastEnemy = 0,
            [InspectorName("HPLoss 아군")]
            LowestHPTeam = 1,
        }

        public enum EDAMAGE_TYPE
        {
            PHYSICAL = 0,
            MAGICAL = 1,
        }

        public enum EBATTLE_RESULT
        {
            DRAW = 0,
            WIN = 1,
            LOSE = 2,
        }

    }
}
