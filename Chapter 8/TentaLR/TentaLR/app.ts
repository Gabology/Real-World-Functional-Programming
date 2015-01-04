/// <reference path="Scripts\typings\jquery\jquery.d.ts" />

function AddElement() {
    var secondRow = document.getElementById("secondRow");
    var td = document.createElement("td");
    td.id = "extracell";
    var button = document.createElement("button");
    button.innerHTML = "DELETE ME";
    button.onclick = () => DeleteElement();
    td.appendChild(button);
    secondRow.appendChild(td);
}

function DeleteElement() {
    var child = <Element>document.getElementById("extracell");
    var parent = document.getElementById("secondRow");
    parent.removeChild(child);
}

window.onload = () => {
    /*  Del 1: 
    Hämta ut noder från DOM
    Hämta ut den första cellen (<td>-elementet) på första raden i dokumentet med hjälp av querySelector. 
    Spara ned den till en variabel som heter ”cell1”. 1p
    Hämta ut den sista cellen (<td>-elementet) på sista raden i dokumentet med hjälp av elementets id. 
    Spara ner den till en variabel som heter ”cell9”. 1p
    Hämta ut den första cellen (<td>-elementet) på andra raden på valfritt sätt med hjälp av jQuery. 
    Spara ner den till en variabel som heter ”cell4”. 1p
    Hämta ut knappen som ligger under tabellen på valfritt sätt och 
    spara ner den till en variabel som heter ”addButton”. 1p
    */

    var cell1 : HTMLElement = <HTMLElement>document.querySelector("#cell1");
    var cell9 = <JQuery><any>document.getElementById("cell9");
    var cell4 = $("#cell4");
    var addButton = <HTMLElement>document.querySelector("#addButton");

    /*
    Manipulera noder med javascript och jQuery
    Ändra innehållet i noden du lagrat i variabeln cell1 genom att manipulera dess inre HTML
     och lägg in texten ”I am Cell1”. Gör detta med Javascript. 1p
    Ändra innehållet i noden du lagrat i variabeln cell9 genom att manipulera dess inre HTML 
     och lägg in texten ”I am Cell9”. Gör detta med jQuery. 1p
    */

    cell1.innerHTML = "I am Cell1";

    /*Hämta ut en samling av noder med javascript
      Hämta ut alla <td> element och spara dem i en variabel som heter ”tds”. 1p
      Kolla men hjälp av en if-sats om storleken på variabeln tds är större än 9. 
      I så fall, logga texten ”Big Array!” till konsolen. Annars, logga texten ”Small Array!” till konsolen. 2p
      
    Hämta ut alla rader med hjälp av klassen ”row” och spara ner dem till en variabel som heter ”rows”. 1p
      Loopa över variabeln ”rows” som är bör vara en array med alla <tr> element. Sätt ett attributet ”style” med värdet ”background-color: yellow” på varje nod med hjälp av loopen.
      Om du har gjort rätt bör alla rader i din tabell ha bakgrundsfärgen gul. 3p */

    var tds = document.getElementsByTagName("td");

    if (tds.length > 9)
        console.log("Big Array!");
    else
        console.log("Small Array!");

    var rows = <NodeListOf<HTMLTableRowElement>>document.getElementsByClassName("row");

    for (var i = 0; i < rows.length; i++)
        rows[i].setAttribute("style", "background-color: yellow");

    addButton.onclick = () => AddElement();

    /*Del 2: VG
    Skapa och ta bort noder
    Skapa en funktion som heter AddElement på valfritt ställe i din javascriptfil. Funktionen ska inte ta emot några argument. 1p
    Skapa en funktion som heter DeleteElement på valfritt ställe i din javascriptfil. Funktionen ska inte ta emot några argument. 1p
    I din AddElement-funktion, hämta ut den andra raden i tabellen (det andra <tr>-elementet) med hjälp av javacript 
    och lagra den i en variabel som du döper till ”secondRow”. 
    Kolla på elementet i html-filen om du behöver tips för hur du kan välja det med javascript. 
    Denna <tr> innehåller ett <td>-element som har texten ”CENTER” i sig , det går alltså inte att ta fel på raden. 1p
    I samma funktion, skapa en ny variabel som heter ”td”. 
    I denna variabel ska du lagra en nod som du skapar med hjälp av javascript. 
    Det element du ska skapa är av typen ”td” som är en datacell. 1p
    I samma funktion, skapa en ny variabel som heter ”button”. 
    I denna variabel ska du lagra en nod som du skapar med hjälp av javascript. 
    Det element du ska skapa är av typen ”button” som är en knapp. 1p
    I den knapp du precis skapat, ändra knappens inre HTML till texten ”DELETE ME”. 1p
    På denna knapp, lägg till en Event Listener som lyssnar efter musklick och kör en funktionen DeleteElement. 2p
    Lägg till denna knapp som ett barn till det <td>-element du tidigare skapat och lagrat i td-variabeln. 1p
    Lägg till denna td-variabel som ett barn till den andra raden du tidigare sparat ner variabeln ”secondRow”. 1p
    Lägg till en Event Listener på den knapp du tidigare sparat som 
    variabeln addButton som lyssnar efter musklick och kör funktionen AddElement. 
    Om du gjort rätt kommer den andra raden ha 4 kolumner där den 4de kolumnen har en knapp som det står ”DELETE ME” på.  
    Det två andra raderna ska endast ha 3 kolumner. 2p
    Koda färdigt DeleteElement-funktionen så att den tar bort hela sin parent när man klickar på knappen 
    (det <td>-element knappen ligger i). 
    Den andra raden ska alltså endast ha 3 kolumner när man klickat på ”DELETE ME”-knappen. 3p
    */

};