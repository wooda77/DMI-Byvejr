#region License
// Copyright (c) 2011 Claus Jørgensen <10229@iha.dk>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
#endregion
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using DMI.Properties;
using System;

namespace DMI.Models
{
    public class Denmark
    {
        public static List<City> Cities
        {
            get
            {
                return
                    Denmark.DenmarkPostalCodes
                        .Select(city => new City()
                        {
                            Name = city.Value,
                            PostalCode = city.Key,
                            Country = "Denmark",
                        })
                        .Concat(Denmark.GreenlandPostalCodes.Select(city => new City()
                        {
                            Name = city.Value,
                            PostalCode = city.Key,
                            Country = "Greenland",
                        }))
                        .Concat(Denmark.FaroeIslandsPostalCodes.Select(city => new City()
                        {
                            Name = city.Value,
                            PostalCode = city.Key,
                            Country = "Faroe Islands",
                        }))
                        .OrderBy(city => city.Name, StringComparer.CurrentCulture)
                        .ToList();
            }
        }

        public static int GetValidPostalCode(int postalCode)
        {
            if (postalCode < 1800)
            {
                return 1000;
            }
            else if (postalCode < 2000)
            {
                return 2000;
            }
            else if (postalCode > 2000 && postalCode < 2500)
            {
                return 1000;
            }
            else if (postalCode > 5000 && postalCode < 5280)
            {
                return 5000;
            }
            else if (postalCode > 6000 && postalCode < 6020)
            {
                return 6000;
            }
            else if (postalCode > 6700 && postalCode < 6720)
            {
                return 6700;
            }
            else if (postalCode > 7100 && postalCode < 7130)
            {
                return 7100;
            }
            else if (postalCode > 8000 && postalCode < 8220)
            {
                return 8000;
            }
            else if (postalCode > 8900 && postalCode < 8950)
            {
                return 8900;
            }
            else if (postalCode == 8920 || postalCode == 8930 || postalCode == 8940 || postalCode == 8960)
            {
                return 8900;
            }
            else if (postalCode > 9000 && postalCode < 9230)
            {
                return 9000;
            }
            else if (postalCode > 9999)
            {
                return 1000;
            }

            return postalCode;
        }

        public static string GetRegionTextFromPostalCode(int postalCode)
        {
            if (postalCode >= 1000 && postalCode <= 2999)
            {
                return AppResources.RegionalText_NorthZealand;
            }
            else if (postalCode >= 3000 && postalCode <= 3699)
            {
                return AppResources.RegionalText_NorthZealand;
            }
            else if (postalCode >= 4000 && postalCode <= 4999)
            {
                return AppResources.RegionalText_SouthZealand;
            }
            else if (postalCode >= 3700 && postalCode <= 3799)
            {
                return AppResources.RegionalText_Bornholm;
            }
            else if (postalCode >= 5000 && postalCode <= 5999)
            {
                return AppResources.RegionalText_Fyn;
            }
            else if (postalCode >= 6000 && postalCode <= 6999)
            {
                return AppResources.RegionalText_SouthJytland;
            }
            else if (postalCode >= 7000 && postalCode <= 7999)
            {
                return AppResources.RegionalText_MiddleJytland;
            }
            else if (postalCode >= 8000 && postalCode <= 8999)
            {
                return AppResources.RegionalText_EastJytland;
            }
            else if (postalCode >= 9000 && postalCode <= 9999)
            {
                return AppResources.RegionalImage_NorthJytland;
            }

            // Default to Copenhagen
            return AppResources.RegionalText_NorthZealand; 
        }

        public static string GetRegionImageFromPostalCode(int postalCode)
        {
            if (postalCode >= 1000 && postalCode <= 2999)
            {
                return AppResources.RegionalImage_NorthZealand;
            }
            else if (postalCode >= 3000 && postalCode <= 3699)
            {
                return AppResources.RegionalImage_NorthZealand;
            }
            else if (postalCode >= 4000 && postalCode <= 4999)
            {
                return AppResources.RegionalImage_SouthZealand;
            }
            else if (postalCode >= 3700 && postalCode <= 3799)
            {
                return AppResources.RegionalImage_Bornholm;
            }
            else if (postalCode >= 5000 && postalCode <= 5999)
            {
                return AppResources.RegionalImage_Fyn;
            }
            else if (postalCode >= 6000 && postalCode <= 6999)
            {
                return AppResources.RegionalImage_SouthJytland;
            }
            else if (postalCode >= 7000 && postalCode <= 7999)
            {
                return AppResources.RegionalImage_MiddleJytland;
            }
            else if (postalCode >= 8000 && postalCode <= 8999)
            {
                return AppResources.RegionalImage_EastJytland;
            }
            else if (postalCode >= 9000 && postalCode <= 9999)
            {
                return AppResources.RegionalImage_NorthJytland;
            }

            // Default to Copenhagen
            return AppResources.RegionalImage_NorthZealand;
        } 

        public static IDictionary<int, string> GreenlandPostalCodes = new Dictionary<int, string>()
        {
            { 4220, "Aasiaat" },
            { 4285, "Angissoq" },
            { 4351, "Aputiteeq" },
            { 9997, "Arsuk" },
            { 4228, "Attu" },
            { 4330, "Daneborg" },
            { 4320, "Danmarkshavn" },
            { 4207, "Hall Land" },
            { 4313, "Henrik Krøyer Holme" },
            { 4373, "Ikermit" },
            { 4382, "Ikermiuarsuk" },
            { 4221, "Ilulissat" },
            { 4339, "Ittoqqortoormiit" },
            { 9998, "Ivittuut" },
            { 9995, "Kangaamiut" },
            { 4825, "Kangaatsiaq" },
            { 4231, "Kangerlussuaq" },
            { 4301, "Kap Morris Jesup" },
            { 4208, "Kitsissorsuit" },
            { 4203, "Kitsissut/Careyøer" },
            { 4241, "Maniitsoq" },
            { 4283, "Nanortalik" },
            { 4280, "Narsaq" },
            { 4270, "Narsarsuaq" },
            { 4266, "Nunarsuit" },
            { 4250, "Nuuk" },
            { 4214, "Nuussuaq" },
            { 4260, "Paamiut" },
            { 4202, "Pituffik" },
            { 4390, "Prins Christian Sund" },
            { 4205, "Qaanaaq" },
            { 4272, "Qaqortoq" },
            { 4817, "Qasigiannguit" },
            { 4219, "Qeqertarsuaq" },
            { 9996, "Qeqertarsuatsiaat" },
            { 9994, "Savissivik" },
            { 4242, "Sioralik" },
            { 4234, "Sisimiut" },
            { 4312, "Station Nord" },
            { 4360, "Tasiilaq" },
            { 9999, "Timmiarmiut" },
            { 4253, "Ukiivik" },
            { 4211, "Upernavik" },
            { 4213, "Uummannaq" },
        };

        public static IDictionary<int, string> FaroeIslandsPostalCodes = new Dictionary<int, string>()
        {
            { 6009, "Akraberg" },            { 6012, "Fugloy" },            { 6005, "Mykines" },            { 6010, "Sørvágur/Vágar" },            { 6011, "Tórshavn" },        };

        public static IDictionary<int, string> DenmarkPostalCodes = new Dictionary<int, string>()
        {
            { 5320, "Agedrup" },
            { 6753, "Agerbæk" },
            { 6534, "Agerskov" },
            { 2620, "Albertslund" },
            { 3450, "Allerød" },
            { 3770, "Allinge/Sandvig" },
            { 8961, "Allingåbro" },
            { 6051, "Almind" },
            { 8592, "Anholt" },
            { 8643, "Ans By" },
            { 6823, "Ansager" },
            { 9510, "Arden" },
            { 4792, "Askeby" },
            { 4550, "Asnæs" },
            { 5466, "Asperup" },
            { 5610, "Assens" },
            { 9340, "Asaa" },
            { 6440, "Augustenborg" },
            { 7490, "Aulum" },
            { 8963, "Auning" },
            { 5935, "Bagenkop" },
            { 2880, "Bagsværd" },
            { 8444, "Balle på Djursland" },
            { 2750, "Ballerup" },
            { 7150, "Barrit" },
            { 8330, "Beder" },
            { 7755, "Bedsted Thy" },
            { 6541, "Bevtoft" },
            { 6852, "Billum" },
            { 7190, "Billund" },
            { 9881, "Bindslev" },
            { 3460, "Birkerød" },
            { 8850, "Bjerringbro" },
            { 6091, "Bjert" },
            { 4632, "Bjæverskov" },
            { 9492, "Blokhus" },
            { 5491, "Blommenslyst" },
            { 6857, "Blåvand" },
            { 4242, "Boeslunde" },
            { 5400, "Bogense" },
            { 4793, "Bogø By" },
            { 6392, "Bolderslev" },
            { 7441, "Bording" },
            { 4791, "Borre" },
            { 4140, "Borup på Sjælland" },
            { 8220, "Brabrand" },
            { 6740, "Bramming" },
            { 7330, "Brande" },
            { 6535, "Branderup J" },
            { 6261, "Bredebro" },
            { 7182, "Bredsten" },
            { 5464, "Brenderup Fyn" },
            { 6310, "Broager" },
            { 5672, "Broby" },
            { 9460, "Brovst" },
            { 8654, "Bryrup" },
            { 8740, "Brædstrup" },
            { 2605, "Brøndby" },
            { 2660, "Brøndby Strand" },
            { 9700, "Brønderslev" },
            { 2700, "Brønshøj" },
            { 6650, "Brørup" },

            { 6372, "Bylderup - Bov" },

            { 6622, "Bække" },

            { 7660, "Bækmarksbro" },

            { 9574, "Bælum" },

            { 7080, "Børkop" },

            { 7650, "Bøvlingbjerg" },

            { 2920, "Charlottenlund" },

            { 6070, "Christiansfeld" },

            { 5380, "Dalby" },

            { 4261, "Dalmose" },

            { 4983, "Dannemare" },

            { 8721, "Daugård" },

            { 4293, "Dianalund" },

            { 2791, "Dragør" },

            { 9330, "Dronninglund" },

            { 3120, "Dronningmølle" },

            { 9352, "Dybvad" },

            { 2870, "Dyssegård" },

            { 5631, "Ebberup" },

            { 8400, "Ebeltoft" },

            { 6320, "Egernsund" },

            { 6040, "Egtved" },

            { 8250, "Egå" },

            { 5592, "Ejby" },

            { 7361, "Ejstrupholm" },

            { 7442, "Engesvang" },

            { 4895, "Errindlev" },

            { 7950, "Erslev" },

            { 6700, "Esbjerg" },

            { 4593, "Eskebjerg" },

            { 4863, "Eskilstrup" },

            { 3060, "Espergærde" },

            { 5600, "Faaborg" },

            { 6720, "Fanø" },

            { 9640, "Farsø" },

            { 3520, "Farum" },

            { 4640, "Faxe" },

            { 4654, "Faxe Ladeplads" },

            { 4944, "Fejø" },

            { 5863, "Ferritslev Fyn" },

            { 4173, "Fjenneslev" },

            { 9690, "Fjerritslev" },

            { 8762, "Flemming" },

            { 3480, "Fredensborg" },

            { 7000, "Fredericia" },

            { 2000, "Frederiksberg" },

            { 9900, "Frederikshavn" },

            { 3600, "Frederikssund" },

            { 3300, "Frederiksværk" },

            { 5871, "Frørup" },

            { 7741, "Frøstrup" },

            { 4250, "Fuglebjerg" },

            { 7884, "Fur" },

            { 4591, "Føllenslev" },

            { 6683, "Føvling" },

            { 4540, "Fårevejle St." },

            { 8990, "Fårup" },

            { 8882, "Fårvang" },

            { 7321, "Gadbjerg" },

            { 4621, "Gadstrup" },

            { 8464, "Galten" },

            { 9362, "Gandrup" },

            { 4874, "Gedser" },

            { 9631, "Gedsted" },

            { 8751, "Gedved" },

            { 5591, "Gelsted" },

            { 2820, "Gentofte" },

            { 6621, "Gesten" },

            { 3250, "Gilleleje" },

            { 5854, "Gislev" },

            { 4532, "Gislinge" },

            { 9260, "Gistrup" },

            { 7323, "Give" },

            { 8983, "Gjerlev J" },

            { 8883, "Gjern" },

            { 5620, "Glamsbjerg" },

            { 6752, "Glejbjerg" },

            { 8585, "Glesborg" },

            { 2600, "Glostrup" },

            { 4171, "Glumsø" },

            { 6510, "Gram" },

            { 6771, "Gredstedbro" },

            { 8500, "Grenå" },

            { 2670, "Greve Strand" },

            { 4571, "Grevinge" },

            { 7200, "Grindsted" },

            { 6300, "Gråsten" },

            { 3230, "Græsted" },

            { 5892, "Gudbjerg Sydfyn" },

            { 3760, "Gudhjem" },

            { 5884, "Gudme" },

            { 4862, "Guldborg" },

            { 6690, "Gørding" },

            { 4281, "Gørlev Sjælland" },

            { 3330, "Gørløse" },

            { 6100, "Haderslev" },

            { 8370, "Hadsten" },

            { 9560, "Hadsund" },

            { 9370, "Hals" },

            { 8450, "Hammel" },

            { 7362, "Hampen" },

            { 7730, "Hanstholm" },

            { 7673, "Harboøre" },

            { 8462, "Harlev J" },

            { 5463, "Harndrup" },

            { 4912, "Harpelunde" },

            { 3790, "Hasle" },

            { 4690, "Haslev" },

            { 8361, "Hasselager" },

            { 4622, "Havdrup" },

            { 8970, "Havndal" },

            { 2640, "Hedehusene" },

            { 8722, "Hedensted" },

            { 6094, "Hejls" },

            { 7250, "Hejnsvig" },

            { 3150, "Hellebæk" },

            { 2900, "Hellerup" },

            { 3200, "Helsinge" },

            { 3000, "Helsingør" },

            { 6893, "Hemmet" },

            { 6854, "Henne" },

            { 4681, "Herfølge" },

            { 2730, "Herlev" },

            { 4160, "Herlufmagle" },

            { 7400, "Herning" },

            { 5874, "Hesselager" },

            { 3400, "Hillerød" },

            { 8382, "Hinnerup" },

            { 9850, "Hirtshals" },

            { 9320, "Hjallerup" },

            { 7560, "Hjerm" },

            { 8530, "Hjortshøj" },

            { 9800, "Hjørring" },

            { 9500, "Hobro" },

            { 4300, "Holbæk" },

            { 4960, "Holeby" },

            { 4684, "Holme-Olstrup" },

            { 7500, "Holstebro" },

            { 6670, "Holsted" },

            { 2840, "Holte" },

            { 4871, "Horbelev" },

            { 3100, "Hornbæk" },

            { 8543, "Hornslet" },

            { 8783, "Hornsyld" },

            { 8700, "Horsens" },

            { 4913, "Horslunde" },

            { 6682, "Hovborg" },

            { 8732, "Hovedgård" },

            { 5932, "Humble" },

            { 3050, "Humlebæk" },

            { 3390, "Hundested" },

            { 7760, "Hurup Thy" },

            { 4330, "Hvalsø" },

            { 7790, "Hvidbjerg" },

            { 6960, "Hvide Sande" },

            { 2650, "Hvidovre" },

            { 8270, "Højbjerg" },

            { 4573, "Højby Sjælland" },

            { 6280, "Højer" },

            { 7840, "Højslev" },

            { 4270, "Høng" },

            { 8362, "Hørning" },

            { 2970, "Hørsholm" },

            { 4534, "Hørve" },

            { 5683, "Haarby" },

            { 4652, "Hårlev" },

            { 4872, "Idestrup" },

            { 7430, "Ikast" },

            { 2635, "Ishøj" },

            { 6851, "Janderup Vestjylland" },

            { 7300, "Jelling" },

            { 9740, "Jerslev J" },

            { 4490, "Jerslev S" },

            { 9981, "Jerup" },

            { 6064, "Jordrup" },

            { 7130, "Juelsminde" },

            { 4450, "Jyderup" },

            { 4040, "Jyllinge" },

            { 4174, "Jystrup Midtsjælland" },

            { 3630, "Jægerspris" },

            { 4400, "Kalundborg" },

            { 4771, "Kalvehave" },

            { 7960, "Karby" },

            { 4653, "Karise" },

            { 2690, "Karlslunde" },

            { 4736, "Karrebæksminde" },

            { 7470, "Karup" },

            { 2770, "Kastrup" },

            { 5300, "Kerteminde" },

            { 4892, "Kettinge" },

            { 6933, "Kibæk" },

            { 4360, "Kirke Eskilstrup" },

            { 4070, "Kirke Hyllinge" },

            { 4060, "Kirke Såby" },

            { 8620, "Kjellerup" },

            { 2930, "Klampenborg" },

            { 9270, "Klarup" },

            { 3782, "Klemensker" },

            { 4672, "Klippinge" },

            { 8765, "Klovborg" },

            { 8420, "Knebel" },

            { 2980, "Kokkedal" },

            { 6000, "Kolding" },

            { 8560, "Kolind" },

            { 2800, "Kongens Lyngby" },

            { 9293, "Kongerslev" },

            { 4220, "Korsør" },

            { 6340, "Kruså" },

            { 3490, "Kvistgård" },

            { 5772, "Kværndrup" },

            { 1000, "København" },

            { 4600, "Køge" },

            { 4772, "Langebæk" },

            { 5550, "Langeskov" },

            { 8870, "Langå" },

            { 4320, "Lejre" },

            { 6940, "Lem St." },

            { 8632, "Lemming" },

            { 7620, "Lemvig" },

            { 4623, "Lille Skensved" },

            { 6660, "Lintrup" },

            { 3360, "Liseleje" },

            { 4750, "Lundby" },

            { 6640, "Lunderskov" },

            { 3540, "Lynge" },

            { 8520, "Lystrup" },

            { 9940, "Læsø/Byrum" },

            { 8831, "Løgstrup" },

            { 9670, "Løgstør" },

            { 6240, "Løgumkloster" },

            { 9480, "Løkken" },

            { 8723, "Løsning" },

            { 8670, "Låsby" },

            { 8340, "Malling" },

            { 9550, "Mariager" },

            { 4930, "Maribo" },

            { 5290, "Marslev" },

            { 5960, "Marstal" },

            { 5390, "Martofte" },

            { 3370, "Melby" },

            { 4735, "Mern" },

            { 5370, "Mesinge Fyn" },

            { 5500, "Middelfart" },

            { 5642, "Millinge" },

            { 5462, "Morud" },

            { 4190, "Munke Bjergby" },

            { 5330, "Munkebo" },

            { 9632, "Møldrup" },

            { 8544, "Mørke" },

            { 4440, "Mørkøv" },

            { 2760, "Måløv" },

            { 8320, "Mårslet" },

            { 4900, "Nakskov" },

            { 3730, "Nexø/Dueodde" },

            { 9240, "Nibe" },

            { 8581, "Nimtofte" },

            { 2990, "Nivå" },

            { 6430, "Nordborg" },

            { 8355, "Ny-Solbjerg" },

            { 5800, "Nyborg" },

            { 4800, "Nykøbing Falster" },

            { 7900, "Nykøbing Mors" },

            { 4500, "Nykøbing Sjælland" },

            { 4880, "Nysted" },

            { 2850, "Nærum" },

            { 4700, "Næstved" },

            { 9610, "Nørager" },

            { 4840, "Nørre Alslev" },

            { 4572, "Nørre Asmindrup" },

            { 5580, "Nørre Aaby" },

            { 6830, "Nørre-Nebel" },

            { 8766, "Nørre-Snede" },

            { 4951, "Nørreballe" },

            { 9400, "Nørresundby" },

            { 8300, "Odder" },

            { 5000, "Odense" },

            { 6840, "Oksbøl" },

            { 5450, "Otterup" },

            { 5883, "Oure" },

            { 6855, "Ovtrup" },

            { 6330, "Padborg" },

            { 9490, "Pandrup" },

            { 4720, "Præstø" },

            { 7183, "Randbøl" },

            { 8900, "Randers" },

            { 9681, "Ranum" },

            { 8763, "Rask Mølle" },

            { 7970, "Redsted M" },

            { 4420, "Regstrup" },

            { 6760, "Ribe" },

            { 5750, "Ringe" },

            { 6950, "Ringkøbing" },

            { 4100, "Ringsted" },

            { 8240, "Risskov" },

            { 4000, "Roskilde" },

            { 7870, "Roslev" },

            { 4243, "Rude" },

            { 5900, "Rudkøbing" },

            { 4291, "Ruds-Vedby" },

            { 2960, "Rungsted Kyst" },

            { 8680, "Ry " },

            { 5350, "Rynkeby" },

            { 8550, "Ryomgård" },

            { 5856, "Ryslinge" },

            { 4970, "Rødby" },

            { 6630, "Rødding" },

            { 6230, "Rødekro" },

            { 8840, "Rødkærsbro" },

            { 2610, "Rødovre" },

            { 4673, "Rødvig Stevns" },

            { 6792, "Rømø/Havneby" },

            { 8410, "Rønde" },

            { 3700, "Rønne" },

            { 4683, "Rønnede" },

            { 4581, "Rørvig" },

            { 8471, "Sabro" },

            { 4990, "Sakskøbing" },

            { 9493, "Saltum" },

            { 8305, "Samsø/Tranebjerg" },

            { 4592, "Sejerø" },

            { 8600, "Silkeborg" },

            { 9870, "Sindal" },

            { 4583, "Sjællands Odde" },

            { 6093, "Sjølund" },

            { 9990, "Skagen" },

            { 8832, "Skals" },

            { 5485, "Skamby" },

            { 8660, "Skanderborg" },

            { 4050, "Skibby" },

            { 7800, "Skive" },

            { 6900, "Skjern" },

            { 2942, "Skodsborg" },

            { 2740, "Skovlunde" },

            { 5881, "Skårup Fyn" },

            { 4230, "Skælskør" },

            { 6780, "Skærbæk" },

            { 3320, "Skævinge" },

            { 8541, "Skødstrup" },

            { 9520, "Skørping" },

            { 4200, "Slagelse" },

            { 3550, "Slangerup" },

            { 2765, "Smørum" },

            { 7752, "Snedsted" },

            { 3070, "Snekkersten" },

            { 4460, "Snertinge" },

            { 2680, "Solrød Strand" },

            { 6560, "Sommersted" },

            { 8641, "Sorring" },

            { 4180, "Sorø" },

            { 8981, "Spentrup" },

            { 6971, "Spjald" },

            { 8472, "Sporup" },

            { 7270, "Stakroge" },

            { 4780, "Stege" },

            { 8781, "Stenderup" },

            { 4295, "Stenlille" },

            { 3660, "Stenløse" },

            { 5771, "Stenstrup" },

            { 4773, "Stensved" },

            { 7850, "Stoholm Jyll." },

            { 4952, "Stokkemarke" },

            { 4480, "Store Fuglede" },

            { 4660, "Store Heddinge" },

            { 4370, "Store Merløse" },

            { 9280, "Storvorde" },

            { 7140, "Stouby" },

            { 9970, "Strandby Vends." },

            { 7600, "Struer" },

            { 4671, "Strøby" },

            { 4850, "Stubbekøbing" },

            { 9530, "Støvring" },

            { 9541, "Suldrup" },

            { 7451, "Sunds" },

            { 3740, "Svaneke" },

            { 4470, "Svebølle" },

            { 5700, "Svendborg" },

            { 9230, "Svenstrup J" },

            { 4520, "Svinninge" },

            { 6470, "Sydals" },

            { 9300, "Sæby" },

            { 2860, "Søborg" },

            { 5985, "Søby Ærø" },

            { 4920, "Søllested" },

            { 7280, "Sønder Felding" },

            { 7260, "Sønder Omme" },

            { 6092, "Sønder Stenderup" },

            { 6400, "Sønderborg" },

            { 5471, "Søndersø" },

            { 7550, "Sørvad" },

            { 2630, "Taastrup" },

            { 4733, "Tappernøje" },

            { 6880, "Tarm" },

            { 9575, "Terndrup" },

            { 8653, "Them" },

            { 7700, "Thisted" },

            { 8881, "Thorsø" },

            { 7680, "Thyborøn" },

            { 3080, "Tikøb" },

            { 8381, "Tilst" },

            { 6980, "Tim" },

            { 6360, "Tinglev" },

            { 6862, "Tistrup" },

            { 3220, "Tisvildeleje" },

            { 6731, "Tjæreborg" },

            { 6520, "Toftlund" },

            { 5690, "Tommerup" },

            { 4891, "Toreby L" },

            { 4943, "Torrig L" },

            { 5953, "Tranekær" },

            { 8570, "Trustrup" },

            { 4030, "Tune" },

            { 4682, "Tureby" },

            { 4340, "Tølløse" },

            { 6270, "Tønder" },

            { 7160, "Tørring" },

            { 9830, "Tårs" },

            { 4350, "Ugerløse" },

            { 7171, "Uldum" },

            { 6990, "Ulfborg" },

            { 5540, "Ullerslev" },

            { 8860, "Ulstrup" },

            { 9430, "Vadum" },

            { 2500, "Valby" },

            { 2625, "Vallensbæk" },

            { 2665, "Vallensbæk Strand" },

            { 6580, "Vamdrup" },

            { 7184, "Vandel" },

            { 2720, "Vanløse" },

            { 6800, "Varde" },

            { 2950, "Vedbæk" },

            { 5474, "Veflinge" },

            { 3210, "Vejby" },

            { 6600, "Vejen" },

            { 6853, "Vejers Strand" },

            { 7100, "Vejle" },

            { 5882, "Vejstrup" },

            { 3670, "Veksø Sjælland" },

            { 7570, "Vemb" },

            { 4241, "Vemmelev" },

            { 7742, "Vesløs" },

            { 9380, "Vestbjerg" },

            { 5762, "Vester Skerninge" },

            { 4953, "Vesterborg" },

            { 7770, "Vestervig" },

            { 8800, "Viborg" },

            { 8260, "Viby J" },

            { 4130, "Viby Sjælland" },

            { 6920, "Videbæk" },

            { 4560, "Vig St." },

            { 7480, "Vildbjerg" },

            { 7980, "Vils" },

            { 7830, "Vinderup" },

            { 4390, "Vipperød" },

            { 2830, "Virum" },

            { 5492, "Vissenbjerg" },

            { 6052, "Viuf" },

            { 9310, "Vodskov" },

            { 6500, "Vojens" },

            { 7173, "Vonge" },

            { 6623, "Vorbasse" },

            { 4760, "Vordingborg" },

            { 9760, "Vrå" },

            { 4873, "Væggerløse" },

            { 3500, "Værløse" },

            { 5970, "Ærøskøbing" },

            { 6870, "Ølgod" },

            { 3310, "Ølsted" },

            { 3650, "Ølstykke" },

            { 5853, "Ørbæk" },

            { 6973, "Ørnhøj" },

            { 8950, "Ørsted" },

            { 8586, "Ørum Djurs" },

            { 8752, "Østbirk" },

            { 4894, "Øster Ulslev" },

            { 7990, "Øster-Assels" },

            { 3751, "Østermarie" },

            { 9750, "Østervrå" },

            { 6200, "Aabenraa" },

            { 9440, "Aabybro" },

            { 8230, "Åbyhøj" },

            { 3720, "Aakirkeby" },

            { 9000, "Aalborg" },

            { 9982, "Ålbæk" },

            { 9620, "Aalestrup" },

            { 3140, "Ålsgårde" },

            { 8000, "Aarhus" },

            { 9600, "Aars" },

            { 5792, "Årslev" },

            { 5560, "Aarup" },
        };
    }
}
