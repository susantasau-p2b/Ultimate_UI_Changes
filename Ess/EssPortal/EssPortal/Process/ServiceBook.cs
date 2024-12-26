using P2b.Global;
using EssPortal.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Payroll;
using EssPortal.Security;
using System.IO;
using EssPortal.Models;
using System.Diagnostics;
using System.Transactions;
using Attendance;
using Leave;
using EMS;
using System.Data.Entity.Infrastructure;


namespace EssPortal.Process
{
    public static class ServiceBook
    {
      
        //Basic Fittment Amount Calculation in promotion activity
        public static double BasicFittmentSelector(double mBasicAmount, IEnumerable<BasicScaleDetails> OBasicScale)
        {

            double mFittmentAmount = 0;
            double mOldBasic = mBasicAmount;//db.P_BasicScaleDetails.Select(e=>e.OldBasic).SingleOrDefault();
            // get current basic in parameter: uses- in case current basic is above endslab i.e. stagnanat
            OBasicScale = OBasicScale.OrderBy(e => e.StartingSlab).ToList();
            Boolean fitment = false;
            foreach (var OBasicScaleRange in OBasicScale)
            {
                if (mOldBasic < OBasicScaleRange.StartingSlab)
                {
                    mFittmentAmount = (OBasicScaleRange.StartingSlab - mOldBasic);
                    fitment = true;
                    break;
                }
                for (int i = 1; i <= OBasicScaleRange.IncrementCount; i++)
                {

                    if (mOldBasic > OBasicScaleRange.StartingSlab && mOldBasic <= (OBasicScaleRange.StartingSlab + (OBasicScaleRange.IncrementAmount * i)))
                    {
                        mFittmentAmount = (OBasicScaleRange.StartingSlab + (OBasicScaleRange.IncrementAmount * i) - mOldBasic);
                        fitment = true;
                        break;

                    }
                }
                if (mFittmentAmount >= 0 && fitment == true)
                {
                    break;
                }
            }

            return mFittmentAmount;
        }
            
    }
}
