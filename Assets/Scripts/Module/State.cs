using System;
using UnityEngine;

namespace KtaneVideoPoker
{
    public enum State
    {
        Idle,
        ShowPayTable,
        FirstDeal,
        ChooseHolds,
        SecondDeal,
        Paying,
        JackpotPending
    }
}
