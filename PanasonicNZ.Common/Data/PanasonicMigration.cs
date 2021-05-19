using PanasonicNZ.Common.Upgrade;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanasonicNZ.Common.Data
{
    public class PanasonicMigration<TContext>
        where TContext : DbContext
    {
        static bool upgradeCompleted = false;
        static object sync = new object();

        public static bool PerformUpgrade(TContext context, bool forceUpgrade)
        {
            if (!upgradeCompleted || forceUpgrade)
            {
                lock (sync)
                {
                    if (!upgradeCompleted || forceUpgrade)
                    {
                        DbUpgrade upgrader = new DbUpgrade(context);
                        if (upgrader.UpgradeNeeded || forceUpgrade)
                            upgrader.UpgradeDatabase(null);

                        upgradeCompleted = true;
                        return true;
                    }
                }
            }

            return false;
        }


        public static bool UpgradeNeeded(TContext context)
        {
            DbUpgrade upgrader = new DbUpgrade(context);
            return upgrader.UpgradeNeeded;
        }
    }
}
