using System;

namespace zMoneyWinX.Model
{
    public static class Signals
    {
        //Delegats
        public delegate void VoidSignalHandler();
        public delegate void TransactionSignalHandler(Transaction item);
        public delegate void MerchantSignalHandler(Merchant item);
        public delegate void TagSignalHandler(Tag item);
        public delegate void AccountSignalHandler(Account item);
        public delegate void GuidSignalHandler(Guid item);
        public delegate void BudgetSignalHandler(Budget item);
        public delegate void BoolSignalHandler(bool item);

        //Signals
        public static event VoidSignalHandler ShowPlannedEvent;
        public static event GuidSignalHandler ShowPlannedGuidEvent;
        public static event BoolSignalHandler ShowPlannedLateEvent;
        public static void ShowPlannedInvoke() => ShowPlannedEvent?.Invoke();
        public static void ShowPlannedInvoke(Guid Id) => ShowPlannedGuidEvent?.Invoke(Id);
        public static void ShowPlannedLateInvoke() => ShowPlannedLateEvent?.Invoke(true);

        public static event VoidSignalHandler CloseMeEvent;
        public static void CloseMeInvoke() => CloseMeEvent?.Invoke();

        public static event VoidSignalHandler IncomeDistrSettingsEvent;
        public static void IncomeDistrSettingsInvoke() => IncomeDistrSettingsEvent?.Invoke();

        public static event VoidSignalHandler AuthOKEvent;
        public static void invokeAuthOK() => AuthOKEvent?.Invoke();
        public static event VoidSignalHandler PinOkEvent;
        public static void invokePinOk() => PinOkEvent?.Invoke();
        public static event VoidSignalHandler SetupPINEvent;
        public static void invokeSetupPIN() => SetupPINEvent?.Invoke();

        public static event VoidSignalHandler SyncStartedEvent;
        public static event VoidSignalHandler SyncEndedEvent;
        public static void invokeSyncStarted() => SyncStartedEvent?.Invoke();
        public static void invokeSyncEnded() => SyncEndedEvent?.Invoke();

        public static event VoidSignalHandler NeedCloseEvent;
        public static void invokeNeedClose() => NeedCloseEvent?.Invoke();
        public static event VoidSignalHandler ConnErrEvent;
        public static void invokeConnErr() => ConnErrEvent?.Invoke();

        public static event VoidSignalHandler LogOutEvent;
        public static void invokeLogOut() => LogOutEvent?.Invoke();

        public static event MerchantSignalHandler MerchantBeginEditEvent;
        public static void MerchantBeginEditInvoke(Merchant item) => MerchantBeginEditEvent?.Invoke(item);

        public static event BudgetSignalHandler BudgetInBeginEditEvent;
        public static void BudgetInBeginEditInvoke(Budget item) => BudgetInBeginEditEvent?.Invoke(item);
        public static event BudgetSignalHandler BudgetOutBeginEditEvent;
        public static void BudgetOutBeginEditInvoke(Budget item) => BudgetOutBeginEditEvent?.Invoke(item);

        public static event TagSignalHandler TagBeginEditEvent;
        public static void TagBeginEditInvoke(Tag item) => TagBeginEditEvent?.Invoke(item);

        public static event TransactionSignalHandler TransactionBeginEditEvent;
        public static void TransactionBeginEditInvoke(Transaction item) => TransactionBeginEditEvent?.Invoke(item);
        public static event VoidSignalHandler FilterEvent;
        public static void invokeFilter() => FilterEvent?.Invoke();

        public static event GuidSignalHandler DeleteTransForAccountEvent;
        public static void DeleteTransForAccountInvoke(Guid item) => DeleteTransForAccountEvent?.Invoke(item);

        public static event VoidSignalHandler UpdateBalanceEvent;
        public static event AccountSignalHandler AccountBeginEditEvent;

        public static void AccountBeginEditInvoke(Account item) => AccountBeginEditEvent?.Invoke(item);
        public static void UpdateBalanceInvoke() => UpdateBalanceEvent?.Invoke();
    }
}
