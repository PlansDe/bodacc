#!/bin/sh
if [! -f bodacc.db ]; then 
create_table
fi

create_table() {
sqlite3 bodacc.db << EOF
CREATE TABLE annonces (ID INT PRIMARY KEY NOT NULL, NUMERO TEXT, DATE TEXT, CODEPOSTAL TEXT, VILLE TEXT, NATURE TEXT, RCS TEXT, TYPE TEXT, FORMEJURIDIQUE);
.quit
EOF
}