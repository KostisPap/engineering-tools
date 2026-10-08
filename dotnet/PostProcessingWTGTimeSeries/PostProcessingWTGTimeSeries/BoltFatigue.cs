using System;


namespace PostProcessingWTGTimeSeries
{
    class BoltFatigue
    {
        public static double BoltDamage(double A, double B, double Count)
        {

            /* Material Properties*/
            //double fub = 1000;        // [MPa]   Bolt ultimate tensile strength
            double fyb = 900;           // [MPa]   Bolt tensile yield strength
            double gammaMFat = 1.00;    // [-]     Flange material resistance factor BS EN 1993-1-9
            double eYMod = 210000;      // [MPa]   Young's Modulus
            double dFF = 3.0;           // [-]     Material resistance 

            /* Flange Dimensions */
            double dFO = 7500;                      // [mm] Diameter at outer wall edge
            double wF = 530;                        // [mm] Flange width incl. wall thickness
            double dFI = dFO - 2 * wF;              // [mm] Diameter at inner flange edge
            // BCD < dFO - 2*snom-2*Bclear-Dwmax
            double BCD = 7010;                      // [mm] Bolt centre diameter
            double noBolts = 138;                   // [mm] Number of bolts
            double sB = Math.PI * BCD / noBolts;    // [mm] Bolt spacing (centre to centre)

            //double dwmax = 128; // [mm] Washer max diameter
            double dwmin = 115; // [mm] Washer min diameter
            double hw = 10;     // [mm] Washer height

            double dBNom = 80;                          // [mm] Nominal bolt hole diameter
            double dBTol = 10;                          // [mm] Bolt hole tolerance
            double dB = dBNom + dBTol;                  // [mm] Bolt hole with tolerance
            //double bClear = 86 + dBTol - dBNom / 2;     // [mm] Bolt to wall clearance
            
            double aBolt = Math.PI * Math.Pow(dBNom, 2) / 4;            // [mm^2]  Area of bolt cross section
            double aS = Math.PI * Math.Pow(dBNom - 0.9382*6, 2) / 4;    // [mm^2]  Tensile stress area of bolt (thread pitch of 6)
            double fPre = 0.7 / 1.1 * fyb * aS;                         // [N]     Bolt Preload (the design standard suggests 0.9 instead of 1/1.1 could be 1% more conservative)

            double sNom = 120;                      // [mm] Wall thickness nominal
            double sCorIn = 0;                      // [mm] Wall corrosion inside
            double sCorOut = 0;                     // [mm] Wall corrosion outside
            double sMin = sNom - sCorIn - sCorOut;  // [mm] Wall thickness
            double dWAxis = dFO - sMin;             // [mm] Diameter at wall centre

            double rfl = 5; // [mm] Flange edge chamfer/radius
            double t = 220; // [mm] Flange thickness
            
            double a = (BCD - dFI) / 2 - rfl;               // [mm] Edge of flange to bolt centre distance
            double b = (dWAxis - BCD) / 2;                  // [mm] Bolt centre to wall axis
            double cw = Math.PI * (dFO - sMin) / noBolts;   // [mm] Wall segment width (arc at wall axis)
            double c = sB;                                  // [mm] Flange segment width (arc length between bolt hole axes)

            /* WTG Tower Section Properties*/
            //double aWall = Math.PI * (dFO - sMin) * sMin;                                       // [mm^2] Area of tower wall (for load calc)
            //double iTW = Math.PI/64 * (Math.Pow(dFO,4)-Math.Pow(dFO-2*sMin,4));                 // [mm^4] Moment of inertia of the tower wall
            double wEl = Math.PI/32 * (Math.Pow(dFO,4)-Math.Pow(dFO-2*sMin,4)) / (dFO-sMin);    // [mm^3] Elastic modulus of the tower bottom section evaluated at the mid thickness of the plate

            /* Fatigue Material Properties*/
            double sClass = 40; // [MPa]    Bolt material fatigue class
            double k = 0.25;    // [-]      Exponent for bolt fatigue (design standard)
            double tRef = 30;   // [mm]     Reference thickness (design standard)

            /* FLS-assessment according to Schmidt-Neuper*/
            // Validity check
            double valid1 = (a + b) / t;
            if (valid1 >= 3 ) { Console.WriteLine("Error in Schmidt-Neuper validity requirement #1"); }

            double lKBolt = 2*t + 2*hw;                                                         // [mm]     Bolt length
            double cS = eYMod * aBolt/(lKBolt + dBNom);                                         // [N/mm]   Bolt stiffeness (VDI 2230 5.1/2 & 5.1/14)
            double dA1 = 2 * c - dB;                                                            // [mm]     Substitutional diameter
            double dA2 = dFO - BCD - 2*sMin;                                                    // [mm]     Substitutional diameter
            double dA3 = BCD - dFI;                                                             // [mm]     Substitutional diameter
            double dA = (dA1+dA2+dA3) / 3;                                                      // [mm]     Mean substitutional (avg of above)
            double dAMin = Math.MinMagnitude(Math.MinMagnitude(dA1, dA2), dA3);                 // [mm]     Min substitutional diameter
            double dAdot = (dA1 + dA2 + dA3 + dAMin) / 4;                                       // [mm]     Substitutional diameter of the basic solid
            double lKClamp = 2 * t;                                                             // [mm]     Clamp length
            double y = dAdot/dwmin;                                                             // [-]      Geometry factor
            double beta = lKClamp/dwmin;                                                        // [-]      Geometry factor
            double phi = 180 / Math.PI * (0.362 + 0.032*Math.Log(beta/2) + 0.153*Math.Log(y));  // [rad]    Substitutional cone angle
            double wCoef = 1.0;                                                                 // [-]      Joint coef. for DVS
            double dAGr = dwmin + wCoef * lKClamp * Math.Tan(phi * Math.PI / 180);              // [mm]     Cone diameter limit
            double cDf = new();                                                                         // [N/m]    Flange stiffness (VDI2230)
            if (dA >= dAGr)
            {
                cDf = wCoef * eYMod * Math.PI * dB * Math.Tan(phi * Math.PI / 180)
                      / 2 / Math.Log((dwmin+dB)*(dwmin+wCoef*lKClamp*Math.Tan(phi*Math.PI/180)-dB)
                                   / (dwmin-dB)*(dwmin+wCoef*lKClamp*Math.Tan(phi*Math.PI/180)+dB));
            }
            else if (dwmin < dA && dA < dAGr)
            {
                cDf = eYMod * Math.PI
                    / (2/wCoef/dB/Math.Tan(phi*Math.PI/180)
                           * Math.Log((dwmin + dB) * (dA - dB) / (dwmin - dB) / (dA + dB))
                       +4/(Math.Pow(dA,2)-Math.Pow(dB,2))  
                           * (lKClamp-(dA-dwmin)/wCoef/Math.Tan(phi*Math.PI/180)));
            }

            double cDw = eYMod*(Math.Pow(dwmin,2)-Math.Pow(dB,2))*Math.PI/4/hw; // [N/mm]   Washer stiffness (Petersen)
            double cD = 1/(1/cDf+2/cDw);                                        // [N/mm]   Flange and washer combined stiffness
            double zI = (a-b/2)/(a+b)*fPre;                                     // [N]      Force limit according to Schmidt-Neuper flange gapping
            //double mbZI = 2*zI*iTW/cw/sMin/dFO / 1000;                          // [Nm]     External bending moment that corresponds to ZI
            double lambda = (0.7*a+b)/0.7/a;                                    // [-]      Stiffness ratio param
            double p = cS/(cD+cS);                                              // [-]      Stiffness ratio param
            double q = cD/(cD+cS);                                              // [-]      Stiffness ratio param
            double zII = fPre/lambda/q;                                         // [N]      Force limit according to Schmidt-Neuper
            //double mbZII = 2*zII*iTW/cw/sMin/dFO/1000;                          // [Nm]     External bending moment that corresponds to ZII

            /* Assumption on the axial load at the interface
               based on an educated guess from analysing a random timeseries*/
            double fAxial = -16199600;                  // [N] Total axial force at the Flanged Connection Interface
            double fAxialSegment = fAxial / noBolts;    // [N] Axial force per bolt segment


            double fInputA = A / wEl * cw * sMin * 1000 + fAxialSegment; // [N] Bolt force from moment A in [Nm]
            double fInputB = B / wEl * cw * sMin * 1000 + fAxialSegment; // [N] Bolt force from moment B in [Nm]

            /* Force limit checks */
            if (fInputA > 0 || fInputB > 0)
            { /* bool test = true; */ }
            else if (fInputA > zI || fInputB > zI)
            { Console.Write("zI limit exceeded"); }
            else if (fInputA > zII || fInputB > zII)
            { Console.Write("zII limit exceeded"); }


            double fSoFA; // [N] Schmidt-Neuper bolt force
            if (fInputA <= 0)       { fSoFA = fPre; }
            else if (fInputA < zI)  { fSoFA = fPre + p * fInputA; }
            else if (fInputA < zII) { fSoFA = fPre + p * zI + (lambda * zII - (fPre + p * zI)) * (fInputA - zI) / (zII - zI); }
            else                    { fSoFA = lambda*fInputA; }

            double fSoFB; // [N] Schmidt-Neuper bolt force
            if (fInputB <= 0)       { fSoFB = fPre; }
            else if (fInputB < zI)  { fSoFB = fPre + p * fInputB; }
            else if (fInputB < zII) { fSoFB = fPre + p * zI + (lambda * zII - (fPre + p * zI)) * (fInputB - zI) / (zII - zI); }
            else                    { fSoFB = lambda * fInputB; }

            double fSRange = Math.Abs(fSoFB - fSoFA); // [N] Bolt force range

            /* Calculation of SN-Curve According to EN 1993-1-9:2010-12 */
            double ks = Math.Pow(tRef/dBNom, k);            // [-]      Size correction factor acc. to Table 8.1 detail 14
            double dSigmaC = sClass;                        // [MPa]    Fatigue category of the bolt material
            double nC = 2 * Math.Pow(10,6);                 // [#]      Number of cycles of 2E6
            double m3 = 3.0;                                // [-]      Slope of the SN curve 
            double dSigmaCRed = ks*dSigmaC/gammaMFat;       // [MPa]    Reduced stress at 2E6
            double nD = 1 * Math.Pow(10, 7);                // [#]      Number of cycles of 1E7
            double m5 = 5.0;                                // [-]      Slope of the SN curve between 1E7 and 1E8
            //double dSigmaD = dSigmaC*Math.Pow(nC/nD, 1/m3); // [MPa]    Stress range at 1E7
            double n1E8 = 1 * Math.Pow(10, 8);              // [#]      Number of cycles of 1E8
            double m20 = 20.0;                              // [-]      Slope of the SN curve beyond 1E8 cycles

            /* Calculation of the reduced SN-Curve parameters*/
            double logAm3 = Math.Log10(nC) + m3 * Math.Log10(dSigmaCRed);       // [-]      Intercept of slope 3 section of the S-N curve with the nocycle axis
            double dSigma3_5 = Math.Pow(10, (logAm3 - Math.Log10(nD)) / m3);    // [MPa]    Stress range at 1E7
            double logAm5 = Math.Log10(nD) + m5 * Math.Log10(dSigma3_5);        // [-]      Intercept of slope 5 section of the S-N curve with the nocycle axis
            double dSigma5_20 = Math.Pow(10, (logAm5 - Math.Log10(n1E8)) / m5); // [MPa]    Stress range at 1E8
            double logAm20 = Math.Log10(n1E8) + m20 * Math.Log10(dSigma5_20);   // [-]      Intercept of slope 20 section of the S-N curve with the nocycle axis


            double dSigmaRange = fSRange / aS; // Bolt stress range
            
            double nDSigmaR; // Number of cycles to failure

            /* Evaluation of number of cycles for the given moment range (moment A -> B) */
            if      (dSigmaRange == 0)          { return 0.0; }
            else if (dSigmaRange < dSigma5_20)  { nDSigmaR = Math.Pow(10, logAm20 - m20 * Math.Log10(dSigmaRange)); }
            else if (dSigmaRange < dSigma3_5)   { nDSigmaR = Math.Pow(10, logAm5 - m5 * Math.Log10(dSigmaRange)); }
            else                                { nDSigmaR = Math.Pow(10, logAm3 - m3 * Math.Log10(dSigmaRange)); }

            double damage = dFF * Count / nDSigmaR;

            return damage;
        }
    }
}
