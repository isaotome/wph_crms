using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.Controls;
using GrapeCity.ActiveReports.SectionReportModel;
using GrapeCity.ActiveReports.Document.Section;
using GrapeCity.ActiveReports.Document;

namespace CrmsReport {
    /// <summary>
    /// CarAppraisalReport の概要の説明です。
    /// </summary>
    public partial class CarAppraisalReport : GrapeCity.ActiveReports.SectionReport
    {

        public CarAppraisalReport()
        {
            //
            // ActiveReport デザイナ サポートに必要です。
            //
            InitializeComponent();
        }

        private void CarAppraisalReport_FetchData(object sender, FetchEventArgs eArgs)
        {
            object irokae = Fields["ChangeColor"].Value;
            object hosyou = Fields["Guarantee"].Value;
            object torisetsu = Fields["Instructions"].Value;

            if (irokae != null)
            {
                switch (irokae.ToString())
                {
                    case "001":
                        this.shapeIrokaeAri.Visible = true;
                        break;
                    case "002":
                        this.shapeIrokaeNasi.Visible = true;
                        break;
                }
            }
            if (hosyou != null)
            {
                switch (hosyou.ToString())
                {
                    case "001":
                        this.shapeHosyouAri.Visible = true;
                        break;
                    case "002":
                        this.shapeHosyouNasi.Visible = true;
                        break;
                }
            }

            if (torisetsu != null)
            {
                switch (torisetsu.ToString())
                {
                    case "001":
                        this.shapeTorisetsuAri.Visible = true;
                        break;
                    case "002":
                        this.shapeTorisetsuNasi.Visible = true;
                        break;
                }
            }

            object steering = Fields["Steering"].Value;
            if (steering != null)
            {
                switch (steering.ToString())
                {
                    case "001":
                        this.shapeSteeringLeft.Visible = true;
                        break;
                    case "002":
                        this.shapeSteeringRight.Visible = true;
                        break;
                }
            }

            object import = Fields["Import"].Value;
            if (import != null)
            {
                switch (steering.ToString())
                {
                    case "001":
                        this.shapeImportD.Visible = true;
                        break;
                    case "002":
                        this.shapeImportHeikou.Visible = true;
                        break;
                }
            }

            object light = Fields["Light"].Value;
            if (light != null)
            {
                switch (light.ToString())
                {
                    case "001":
                        this.shapeLightHalo.Visible = true;
                        break;
                    case "002":
                        this.shapeLightHID.Visible = true;
                        break;
                }
            }

            object aw = Fields["AW"].Value;
            if (aw != null)
            {
                switch (aw.ToString())
                {
                    case "001":
                        this.shapeAWJun.Visible = true;
                        break;
                    case "002":
                        this.shapeAWGai.Visible = true;
                        break;
                }
            }

            object aero = Fields["Aero"].Value;
            if (aero != null)
            {
                switch (aero.ToString())
                {
                    case "001":
                        this.shapeAeroJun.Visible = true;
                        break;
                    case "002":
                        this.shapeAeroGai.Visible = true;
                        break;
                }
            }

            object cd = Fields["Cd"].Value;
            if (cd != null)
            {
                switch (cd.ToString())
                {
                    case "001":
                        this.shapeCdJun.Visible = true;
                        break;
                    case "002":
                        this.shapeCdGai.Visible = true;
                        break;
                }
            }

            object md = Fields["Md"].Value;
            if (md != null)
            {
                switch (md.ToString())
                {
                    case "001":
                        this.shapeMdJun.Visible = true;
                        break;
                    case "002":
                        this.shapeMdGai.Visible = true;
                        break;
                }
            }

            object naviType = Fields["NaviType"].Value;
            if (naviType != null)
            {
                switch (naviType.ToString())
                {
                    case "001":
                        this.shapeNaviJun.Visible = true;
                        break;
                    case "002":
                        this.shapeNaviGai.Visible = true;
                        break;
                }
            }

            object naviDash = Fields["NaviDashboard"].Value;
            if (naviDash != null)
            {
                switch (naviDash.ToString())
                {
                    case "001":
                        this.shapeNaviOnDash.Visible = true;
                        break;
                    case "002":
                        this.shapeNaviInDash.Visible = true;
                        break;
                }
            }

            object recycle = Fields["Recycle"].Value;
            if (recycle != null)
            {
                switch (recycle.ToString())
                {
                    case "001":
                        this.shapeRecycleYotaku.Visible = true;
                        break;
                    case "002":
                        this.shapeRecycleMiYotaku.Visible = true;
                        break;
                }
            }

            object recycleKen = Fields["RecycleTicket"].Value;
            if (recycleKen != null)
            {
                switch (recycleKen.ToString())
                {
                    case "001":
                        this.shapeRecycleKenAri.Visible = true;
                        break;
                    case "002":
                        this.shapeRecycleKenNasi.Visible = true;
                        break;
                }
            }

            object seatType = Fields["SeatType"].Value;
            if (seatType != null)
            {
                switch (seatType.ToString())
                {
                    case "001":
                        this.shapeSeatKawa.Visible = true;
                        break;
                    case "002":
                        this.shapeSeatFuchiKawa.Visible = true;
                        break;
                    case "003":
                        this.shapeSeatCombiKawa.Visible = true;
                        break;
                    case "004":
                        this.shapeSeatShagai.Visible = true;
                        break;
                }
            }

            object reparationRecord = Fields["ReparationRecord"].Value;
            if (reparationRecord != null)
            {
                switch (reparationRecord.ToString())
                {
                    case "001":
                        this.shapeShufukuAri.Visible = true;
                        break;
                    case "002":
                        this.shapeShufukuNasi.Visible = true;
                        break;
                }
            }

            object mileageUnit1 = Fields["AppraisalMileageUnit"].Value;
            if (mileageUnit1 != null)
            {
                switch (mileageUnit1.ToString())
                {
                    case "001":
                        this.shapeKm.Visible = true;
                        break;
                    case "002":
                        this.shapeMile.Visible = true;
                        break;
                }
            }
        }

        private void CarAppraisalReport_ReportStart(object sender, EventArgs e)
        {
            // Setup Virtual Printer and Paper Size(A4/Portrait).
            this.Document.Printer.PrinterName = "";
            this.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A4;
            this.PageSettings.Orientation = GrapeCity.ActiveReports.Document.Section.PageOrientation.Portrait;
        }
    }
}
