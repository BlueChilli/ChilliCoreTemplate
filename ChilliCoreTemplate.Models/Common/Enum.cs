using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models
{
    public enum Country
    {
        [Description("Afghanistan")]
        AF = 1,
        [Description("Åland Islands")]
        AX,
        [Description("Albania")]
        AL,
        [Description("Algeria")]
        DZ,
        [Description("American Samoa")]
        AS,
        [Description("Andorra")]
        AD,
        [Description("Angola")]
        AO,
        [Description("Anguilla")]
        AI,
        [Description("Antarctica")]
        AQ,
        [Description("Antigua and Barbuda")]
        AG,
        [Description("Argentina")]
        AR,
        [Description("Armenia")]
        AM,
        [Description("Aruba")]
        AW,
        [Description("Australia")]
        AU,
        [Description("Austria")]
        AT,
        [Description("Azerbaijan")]
        AZ,
        [Description("Bahamas")]
        BS,
        [Description("Bahrain")]
        BH,
        [Description("Bangladesh")]
        BD,
        [Description("Barbados")]
        BB,
        [Description("Belarus")]
        BY,
        [Description("Belgium")]
        BE,
        [Description("Belize")]
        BZ,
        [Description("Benin")]
        BJ,
        [Description("Bermuda")]
        BM,
        [Description("Bhutan")]
        BT,
        [Description("Bolivia, Plurinational State of")]
        BO,
        [Description("Bonaire, Sint Eustatius and Saba")]
        BQ,
        [Description("Bosnia and Herzegovina")]
        BA,
        [Description("Botswana")]
        BW,
        [Description("Bouvet Island")]
        BV,
        [Description("Brazil")]
        BR,
        [Description("British Indian Ocean Territory")]
        IO,
        [Description("Brunei Darussalam")]
        BN,
        [Description("Bulgaria")]
        BG,
        [Description("Burkina Faso")]
        BF,
        [Description("Burundi")]
        BI,
        [Description("Cambodia")]
        KH,
        [Description("Cameroon")]
        CM,
        [Description("Canada")]
        CA,
        [Description("Cape Verde")]
        CV,
        [Description("Cayman Islands")]
        KY,
        [Description("Central African Republic")]
        CF,
        [Description("Chad")]
        TD,
        [Description("Chile")]
        CL,
        [Description("China")]
        CN,
        [Description("Christmas Island")]
        CX,
        [Description("Cocos (Keeling) Islands")]
        CC,
        [Description("Colombia")]
        CO,
        [Description("Comoros")]
        KM,
        [Description("Congo")]
        CG,
        [Description("Congo, the Democratic Republic of the")]
        CD,
        [Description("Cook Islands")]
        CK,
        [Description("Costa Rica")]
        CR,
        [Description("Côte d'Ivoire")]
        CI,
        [Description("Croatia")]
        HR,
        [Description("Cuba")]
        CU,
        [Description("Curaçao")]
        CW,
        [Description("Cyprus")]
        CY,
        [Description("Czech Republic")]
        CZ,
        [Description("Denmark")]
        DK,
        [Description("Djibouti")]
        DJ,
        [Description("Dominica")]
        DM,
        [Description("Dominican Republic")]
        DO,
        [Description("Ecuador")]
        EC,
        [Description("Egypt")]
        EG,
        [Description("El Salvador")]
        SV,
        [Description("Equatorial Guinea")]
        GQ,
        [Description("Eritrea")]
        ER,
        [Description("Estonia")]
        EE,
        [Description("Ethiopia")]
        ET,
        [Description("Falkland Islands (Malvinas)")]
        FK,
        [Description("Faroe Islands")]
        FO,
        [Description("Fiji")]
        FJ,
        [Description("Finland")]
        FI,
        [Description("France")]
        FR,
        [Description("French Guiana")]
        GF,
        [Description("French Polynesia")]
        PF,
        [Description("French Southern Territories")]
        TF,
        [Description("Gabon")]
        GA,
        [Description("Gambia")]
        GM,
        [Description("Georgia")]
        GE,
        [Description("Germany")]
        DE,
        [Description("Ghana")]
        GH,
        [Description("Gibraltar")]
        GI,
        [Description("Greece")]
        GR,
        [Description("Greenland")]
        GL,
        [Description("Grenada")]
        GD,
        [Description("Guadeloupe")]
        GP,
        [Description("Guam")]
        GU,
        [Description("Guatemala")]
        GT,
        [Description("Guernsey")]
        GG,
        [Description("Guinea")]
        GN,
        [Description("Guinea-Bissau")]
        GW,
        [Description("Guyana")]
        GY,
        [Description("Haiti")]
        HT,
        [Description("Heard Island and McDonald Islands")]
        HM,
        [Description("Holy See (Vatican City State)")]
        VA,
        [Description("Honduras")]
        HN,
        [Description("Hong Kong")]
        HK,
        [Description("Hungary")]
        HU,
        [Description("Iceland")]
        IS,
        [Description("India")]
        IN,
        [Description("Indonesia")]
        ID,
        [Description("Iran, Islamic Republic of")]
        IR,
        [Description("Iraq")]
        IQ,
        [Description("Ireland")]
        IE,
        [Description("Isle of Man")]
        IM,
        [Description("Israel")]
        IL,
        [Description("Italy")]
        IT,
        [Description("Jamaica")]
        JM,
        [Description("Japan")]
        JP,
        [Description("Jersey")]
        JE,
        [Description("Jordan")]
        JO,
        [Description("Kazakhstan")]
        KZ,
        [Description("Kenya")]
        KE,
        [Description("Kiribati")]
        KI,
        [Description("Korea, Democratic People's Republic of")]
        KP,
        [Description("Korea, Republic of")]
        KR,
        [Description("Kuwait")]
        KW,
        [Description("Kyrgyzstan")]
        KG,
        [Description("Lao People's Democratic Republic")]
        LA,
        [Description("Latvia")]
        LV,
        [Description("Lebanon")]
        LB,
        [Description("Lesotho")]
        LS,
        [Description("Liberia")]
        LR,
        [Description("Libya")]
        LY,
        [Description("Liechtenstein")]
        LI,
        [Description("Lithuania")]
        LT,
        [Description("Luxembourg")]
        LU,
        [Description("Macao")]
        MO,
        [Description("Macedonia, the former Yugoslav Republic of")]
        MK,
        [Description("Madagascar")]
        MG,
        [Description("Malawi")]
        MW,
        [Description("Malaysia")]
        MY,
        [Description("Maldives")]
        MV,
        [Description("Mali")]
        ML,
        [Description("Malta")]
        MT,
        [Description("Marshall Islands")]
        MH,
        [Description("Martinique")]
        MQ,
        [Description("Mauritania")]
        MR,
        [Description("Mauritius")]
        MU,
        [Description("Mayotte")]
        YT,
        [Description("Mexico")]
        MX,
        [Description("Micronesia, Federated States of")]
        FM,
        [Description("Moldova, Republic of")]
        MD,
        [Description("Monaco")]
        MC,
        [Description("Mongolia")]
        MN,
        [Description("Montenegro")]
        ME,
        [Description("Montserrat")]
        MS,
        [Description("Morocco")]
        MA,
        [Description("Mozambique")]
        MZ,
        [Description("Myanmar")]
        MM,
        [Description("Namibia")]
        NA,
        [Description("Nauru")]
        NR,
        [Description("Nepal")]
        NP,
        [Description("Netherlands")]
        NL,
        [Description("New Caledonia")]
        NC,
        [Description("New Zealand")]
        NZ,
        [Description("Nicaragua")]
        NI,
        [Description("Niger")]
        NE,
        [Description("Nigeria")]
        NG,
        [Description("Niue")]
        NU,
        [Description("Norfolk Island")]
        NF,
        [Description("Northern Mariana Islands")]
        MP,
        [Description("Norway")]
        NO,
        [Description("Oman")]
        OM,
        [Description("Pakistan")]
        PK,
        [Description("Palau")]
        PW,
        [Description("Palestine, State of")]
        PS,
        [Description("Panama")]
        PA,
        [Description("Papua New Guinea")]
        PG,
        [Description("Paraguay")]
        PY,
        [Description("Peru")]
        PE,
        [Description("Philippines")]
        PH,
        [Description("Pitcairn")]
        PN,
        [Description("Poland")]
        PL,
        [Description("Portugal")]
        PT,
        [Description("Puerto Rico")]
        PR,
        [Description("Qatar")]
        QA,
        [Description("Réunion")]
        RE,
        [Description("Romania")]
        RO,
        [Description("Russian Federation")]
        RU,
        [Description("Rwanda")]
        RW,
        [Description("Saint Barthélemy")]
        BL,
        [Description("Saint Helena, Ascension and Tristan da Cunha")]
        SH,
        [Description("Saint Kitts and Nevis")]
        KN,
        [Description("Saint Lucia")]
        LC,
        [Description("Saint Martin (French part)")]
        MF,
        [Description("Saint Pierre and Miquelon")]
        PM,
        [Description("Saint Vincent and the Grenadines")]
        VC,
        [Description("Samoa")]
        WS,
        [Description("San Marino")]
        SM,
        [Description("Sao Tome and Principe")]
        ST,
        [Description("Saudi Arabia")]
        SA,
        [Description("Senegal")]
        SN,
        [Description("Serbia")]
        RS,
        [Description("Seychelles")]
        SC,
        [Description("Sierra Leone")]
        SL,
        [Description("Singapore")]
        SG,
        [Description("Sint Maarten (Dutch part)")]
        SX,
        [Description("Slovakia")]
        SK,
        [Description("Slovenia")]
        SI,
        [Description("Solomon Islands")]
        SB,
        [Description("Somalia")]
        SO,
        [Description("South Africa")]
        ZA,
        [Description("South Georgia and the South Sandwich Islands")]
        GS,
        [Description("South Sudan")]
        SS,
        [Description("Spain")]
        ES,
        [Description("Sri Lanka")]
        LK,
        [Description("Sudan")]
        SD,
        [Description("Suriname")]
        SR,
        [Description("Svalbard and Jan Mayen")]
        SJ,
        [Description("Swaziland")]
        SZ,
        [Description("Sweden")]
        SE,
        [Description("Switzerland")]
        CH,
        [Description("Syrian Arab Republic")]
        SY,
        [Description("Taiwan, Province of China")]
        TW,
        [Description("Tajikistan")]
        TJ,
        [Description("Tanzania, United Republic of")]
        TZ,
        [Description("Thailand")]
        TH,
        [Description("Timor-Leste")]
        TL,
        [Description("Togo")]
        TG,
        [Description("Tokelau")]
        TK,
        [Description("Tonga")]
        TO,
        [Description("Trinidad and Tobago")]
        TT,
        [Description("Tunisia")]
        TN,
        [Description("Turkey")]
        TR,
        [Description("Turkmenistan")]
        TM,
        [Description("Turks and Caicos Islands")]
        TC,
        [Description("Tuvalu")]
        TV,
        [Description("Uganda")]
        UG,
        [Description("Ukraine")]
        UA,
        [Description("United Arab Emirates")]
        AE,
        [Description("United Kingdom")]
        GB,
        [Description("United States")]
        US,
        [Description("United States Minor Outlying Islands")]
        UM,
        [Description("Uruguay")]
        UY,
        [Description("Uzbekistan")]
        UZ,
        [Description("Vanuatu")]
        VU,
        [Description("Venezuela, Bolivarian Republic of")]
        VE,
        [Description("Viet Nam")]
        VN,
        [Description("Virgin Islands, British")]
        VG,
        [Description("Virgin Islands, U.S.")]
        VI,
        [Description("Wallis and Futuna")]
        WF,
        [Description("Western Sahara")]
        EH,
        [Description("Yemen")]
        YE,
        [Description("Zambia")]
        ZM,
        [Description("Zimbabwe")]
        ZW
    }

    public enum BreadcrumbStatus
    {
        Plain,
        Primary,
        [Description("info")]
        Information,
        Success,
        Warning,
        Danger,
    }

}
