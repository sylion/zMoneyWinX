using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using zMoneyWinX.Model;

namespace zMoneyWinX.Client
{
    public class SyncManager
    {
        public static async Task<bool> Sync(bool fullSync = false)
        {
            await DBInitializer.initDB();
            DiffResponseObject DO = new DiffResponseObject();
            bool haserror = false;
            await Task.Run(async () =>
            {
                try
                {
                    DO = await Client.diff();
                }
                catch (Exception error)
                {
                    await new MessageDialog(error.Message).ShowAsync();
                    haserror = true;
                }
            });

            if (haserror)
            {
                Signals.invokeConnErr();
                return false;
            }

            if (DO == null)
                return true;

            //Deletions
            if (DO.Deletions != null)
                App.Provider.Deletions(DO.Deletions);

            //Changes
            if (DO.Users != null)
                DBObject.Insert(DO.Users);

            if (DO.Countries != null)
                DBObject.Insert(DO.Countries);

            if (DO.Merchants != null)
            {
                if (fullSync)
                    DBObject.Insert(Merchant.CreateMetadata(DO.Merchants));
                else
                    App.Provider.VMMerchant.Insert(Merchant.CreateMetadata(DO.Merchants));
            }

            if (DO.Instruments != null)
            {
                if (fullSync)
                    DBObject.Insert(DO.Instruments);
                else
                    Instrument.UpdateRates(DO.Instruments);
            }

            if (DO.Tags != null)
            {
                if (fullSync)
                    DBObject.Insert(DO.Tags);
                else
                    App.Provider.VMTag.Insert(DO.Tags);
            }
            if (DO.Companies != null)
                DBObject.Insert(DO.Companies);

            if (DO.Budgets != null)
                DBObject.Insert(Budget.createMetaData(DO.Budgets));

            if (DO.Accounts != null)
            {
                if (fullSync)
                    DBObject.Insert(Account.createMetaData(DO.Accounts));
                else
                    App.Provider.VMAccount.Insert(Account.createMetaData(DO.Accounts));
            }

            if (DO.Reminders != null)
                DBObject.Insert(DO.Reminders, fullSync);
            if (DO.ReminderMarkers != null)
                DBObject.Insert(await ReminderMarker.createMetaData(DO.ReminderMarkers, DO.Accounts));

            if (DO.Transactions != null)
            {
                if (fullSync)
                    DBObject.Insert(await Transaction.createMetaData(DO.Transactions, DO.Accounts));
                else if (DO.Transactions.Count < 100)
                    App.Provider.VMTransaction.Update(await Transaction.createMetaData(DO.Transactions));
                else
                {
                    DBObject.Insert(await Transaction.createMetaData(DO.Transactions));
                    await App.Provider.VMTransaction.Refresh();
                }
            }
            //Add 1 millisecond to the timestamp, because server use >= condition and returns items which allready synced
            SettingsManager.lastsynctime = DO.ServerTimeStamp + 1;
            return true;
        }
    }
}