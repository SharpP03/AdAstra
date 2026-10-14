---
title: Dziennik decyzji do pracy inżynierskiej
description: >
  Archiwalny, chronologiczny zapis kwestii istotnych dla pracy inżynierskiej:
  decyzje projektowe, techniczne i architektoniczne, problemy, które się pojawiły,
  oraz wybrane podejście (z odrzuconymi alternatywami, jeśli były). Nie jest źródłem
  aktualnych założeń – te opisuje Assets/DOCS/GAMEPLAY.md i kod.
scope: >
  Dziennik opisuje drogę: jak doszliśmy do rozwiązania i dlaczego. Opis działania
  gotowej architektury należy do Assets/DOCS/THESIS_NOTES.md, a surowe notatki
  i pomiary z playtestów do Assets/DOCS/raports/playtests/ – wpis odsyła do nich
  zamiast je powtarzać (np. „playtest pokazał X, więc zmieniono Y”).
agents: >
  Tylko dopisuj. Nie czytaj wpisów i nie traktuj ich jako wytycznych przy pracy ani
  projektowaniu. Nie zmieniaj istniejących wpisów. Nowy wpis dodawaj na końcu pliku,
  gdy zadanie przyniosło decyzję, problem lub podejście warte opisania w pracy.
entry_format: |
  ## <czego dotyczy>, <branch>, <RRRR-MM-DD>
  * **Problem / kontekst:** co było nie tak albo co trzeba było rozstrzygnąć.
  * **Decyzja / podejście:** co wybrano i jak to działa.
  * **Dlaczego:** uzasadnienie; odrzucone alternatywy, jeśli były.
  * **Powiązane:** taski (#N), kluczowe pliki lub sekcje dokumentacji.
  Zwięźle; dłuższy opis tylko wtedy, gdy temat tego wymaga.
---

# DESIGN DECISIONS – DZIENNIK DECYZJI PROJEKTOWYCH

---

## 2026-10-14 – Cel projektowy, rdzenie jako inwestycja, układ sektora bez sztywnych beatów

Wynik sesji przeglądu stanu projektu i grilla game designu przed integracją sektora (#73).

### 1. Cel projektowy: 3 filary i wspólna waluta
* **Przed:** brak wspólnego kryterium; każda nowa mechanika wprowadzała własny zasób lub pomysł (energia, materiały, prądy jonowe – #81–#83), backlog rósł bez filtra.
* **Po:** 3 filary (ucieczka na budżecie; lot sam w sobie daje frajdę; ryzyko z wyboru, czasem w ciemno), dystans do fali jako wspólna waluta i kontrakt lokacji (koszt w dystansie, nagroda, filar) – GAMEPLAY.md 1.1.
* **Dlaczego:** filtr na pomysły, mierzalne pytania w playteście i strojenie jedną „gałką”. Rozważano jeden filar – odrzucony jako zbyt wąski; wspólna waluta zostaje, filary są trzy.

### 2. Prezentacja pracy a stan gry
* **Przed:** prezentacja zakłada eksplorację, zdarzenia losowe i regrywalność (proceduralne lokacje); gra to autorski, liniowy sektor.
* **Po:** najpierw domknięcie istniejącej pętli; losowość i lokacje nieznane jako kierunek długoterminowy (#88). Pytanie badawcze pracy może się jeszcze zmienić.
* **Dlaczego:** czas (ok. 8–10 tygodni) i brak grywalnego runu; prezentacja jest poglądowa.

### 3. Rdzenie Stazy: z rzadkości na koszt alternatywny
* **Przed:** 3 rdzenie w sektorze, 2 wymagane; dylemat opierał się na rzadkości („trzeci rdzeń to zapas”).
* **Po:** w sektorze więcej rdzeni niż wymaga brama; rdzeń jest kluczem, ratunkiem (Staza) i inwestycją – nadmiar dowieziony przez bramę idzie na ulepszenia między runami (#52).
* **Dlaczego:** każda Staza ma koszt także przy pewnych kluczach; wprawny gracz dostaje nagrodę za zebranie wszystkich rdzeni; powód do kolejnego runu.

### 4. Meta-progresja przeniesiona do MVP (wersja minimalna)
* **Przed:** meta-progresja POST-MVP.
* **Po:** bankowanie nadmiaru rdzeni tylko po udanym skoku, menu ulepszeń, ulepszenia umiarkowane lub nowe narzędzia (unik, SOS, laser górniczy, skaner). Persystencja do osobnego grilla.
* **Dlaczego:** niski koszt implementacji przy dużym zysku dla regrywalności; umiar chroni presję fali.

### 5. Układ sektora: lokacje główne i moduły zamiast tabeli beatów
* **Przed:** sektor jako stała sekwencja beatów i odcinków tranzytowych (tabela poniżej); stacja była beatem.
* **Po:** beat = lokacja główna (duża, wyrazista, z własną mechaniką); stacja to moduł usługowy, którego pozycję wyznacza budżet paliwa; moduły opcjonalne mogą stać w przerwach, przy lokacjach głównych i w nich; rozmieszczenie według reguł z GAMEPLAY.md 2.5, korygowane playtestem.
* **Dlaczego:** sztywna tabela narzucała układ, zanim playtest cokolwiek pokazał; reguły zamiast tabeli są też tym, czego potrzebuje przyszły generator sektora.

#### Archiwum: tabela beatów (MVP) sprzed zmiany

| Beat | Rola | Moduł | Napięcie |
|------|------|-------|----------|
| Start | Nauka lotu; fala daleko w tyle | Hub | Niskie |
| Odcinek 1 | Nauka zderzeń; pierwszy Rdzeń Stazy w gęstym środku pola – nauka klucza i Stazy | Pole asteroid | Średnie |
| Stacja | Tankowanie w wiązce pod presją fali; pierwszy dylemat: dotankować kosztem spalenia rdzenia? | Stacja pośrednia | Rosnące |
| Odcinek 2 | Test umiejętności i wybór strefy; strumień w poprzek trasy, burza i anomalia obok niej; rdzenie w strumieniu i burzy (docelowo także anomalia z wyrzutem jako skrótem) | Strumień Asteroid, burza jonowa, anomalia grawitacyjna | Wysokie |
| Finisz | Brama w zasięgu wzroku; wyścig z falą o resztki paliwa, zapasowy rdzeń jako Staza na procedurę skoku | Warp Gate | Szczyt |

Odcinki tranzytowe (sprzed zmiany): Hub → pole, pole → stacja, stacja → odcinek 2, odcinek 2 → brama; każdy z autorskim pasmem i modułami samodzielnymi.

### 6. Rdzenie poza strefami zagrożeń – reguła złagodzona
* **Przed:** rdzenie wyłącznie w strefach zagrożeń; zakaz rdzeni w tranzycie i modułach.
* **Po:** rdzenie przeważnie w miejscach o dużym zagrożeniu; inne źródło dopuszczalne, jeśli uzasadnia je game design (np. nagroda z losowego eventu).
* **Dlaczego:** zakaz blokował przyszłe lokacje nieznane i eventy; kosztowny dostęp do rdzenia pozostaje zasadą domyślną.

### 7. Pozostałe ustalenia
* Stacja bez zmian w wycinku – czas tankowania (ok. 19 s) sprawdza playtest (#76).
* Boki sektora bez granicy – fala sama karze lot w bok (#58 zostaje w Ideas).
* Rozbudowa lokacji dopiero po playteście; pytanie „która lokacja jest najuboższa” w #76.
* MVP to jeden sektor; centrum galaktyki jako fikcja i kierunek (#91 – run z wielu skoków).

## Pierwszy przelot sektora: długość runu, Staza, paliwo z modułów, feat/sector-world-build, 2026-10-14
* **Problem / kontekst:** pierwszy przelot przebudowanego sektora (`playtests/2026-10-14-sector-blockout.md`) pokazał:
  * run jest za krótki;
  * nadmiarowy rdzeń się nie opłaca: Staza 6 s oddawała ~100 m, a zboczenie po rdzeń kosztuje kilkaset metrów;
  * skrzynie dają mało, a paliwa nie daje nic poza stacją;
  * boost opłacał się zawsze.
* **Decyzja / podejście:**
  * dłuższy sektor (brama ~3200 m) zamiast wolniejszego statku;
  * Staza ~25 s, czyli mniej więcej koszt jednego zboczenia; prędkość fali bez zmian, żeby nie ruszać dwóch dźwigni naraz;
  * boost droższy na metr (×2 prędkości za ×3,2 paliwa);
  * skrzynie paliwa (10% baku) z autorskim miksem typów w wrakach;
  * twarde reguły „salvage < 1 rdzeń” i „paliwo z modułów < deficyt” zamienione na szacunki;
  * z GAMEPLAY usunięty cel 2–3 min: długość runu wynika z układu i zostawia czas na eksplorację.
* **Dlaczego:**
  * odrzucona Staza 60 s: ~1 km, czyli połowa sektora; jeden rdzeń kasowałby presję fali;
  * wolniejszy statek psuje frajdę z lotu (filar 2);
  * twarde limity przy prototypie blokowały strojenie na wyczucie;
  * losowanie typu skrzyń odłożone do losowania sektora;
  * sprawdzone zostało, że propozycja ×2 prędkości za ×2,3 paliwa obniżyłaby koszt boostu na metr (1,22 → 1,15), czyli pogłębiłaby problem.
* **Powiązane:** #73, #74 (wymóg rdzeni na bramie do rozstrzygnięcia), #95 (skala i wypełnienie pustki), #96 (przegląd modelu lotu), GAMEPLAY.md 1, 2.1, 2.5, 4.1, 4.3, 4.6.

## Kamera podążająca za kwaternionem statku, feat/player-flight-model-review, 2026-10-14
* **Problem / kontekst:** playtest pokazał szarpanie kamery. Roll czytany z kątów Eulera (`eulerAngles.z`) razem z `LookRotation(forward, Vector3.up)` przeskakiwał przy pionowym locie, a przechył zależny od surowego wejścia (A/D, W/S) zmieniał cel skokowo przy wciśnięciu klawisza i przechylał kamerę także w trybie realistycznym, w którym A/D nie porusza statkiem.
* **Decyzja / podejście:** kamera podąża za pełną rotacją statku (kwaternion) z wygładzaniem 1 − e^(−k·dt); przechył od wejścia usunięto.
* **Dlaczego:** usuwa skoki i przesunięcie na A/D, a wygładzanie wykładnicze daje ten sam efekt niezależnie od liczby klatek.
* **Powiązane:** #96, `DynamicCamera.cs`.

## Dryf statku po wyczerpaniu paliwa, feat/player-flight-model-review, 2026-10-14
* **Problem / kontekst:** jeden opór liniowy sprawiał, że po wyczerpaniu paliwa statek zatrzymywał się po kilku sekundach jak samochód. Ratunkiem było praktycznie tylko skończenie paliwa już w strefie stacji lub bramy.
* **Decyzja / podejście:** dwa poziomy oporu. Przy działających silnikach opór ogranicza prędkość maksymalną (ok. ciąg / opór), a po puszczeniu ciągu statek hamuje. Przy pustym baku opór spada prawie do zera i statek dryfuje na resztce pędu (droga ok. v / opór), ale skończenie. Stacja wyhamowuje w swojej strefie statek bez paliwa, żeby mógł zatankować; statek z paliwem przelatuje bez hamowania.
* **Dlaczego:** daje decyzję „ostatnie paliwo na boost i wystrzelenie się w stronę stacji”, celując przed wygaśnięciem silników. Odrzucono niski opór przy każdym braku ciągu – opłacałoby się „pyknąć” ciąg i lecieć za darmo, a paliwo przestałoby być budżetem dystansu.
* **Powiązane:** #96, `Player_Spaceship.cs`, `ShipFlightConfigSO.cs`, `RefuelStation.cs`, `GAMEPLAY.md` 3.2 i 4.2.

## Wyrzut z anomalii grawitacyjnej, feat/gravity-anomaly-rings, 2026-10-14
* **Problem / kontekst:** anomalia ciągnęła najmocniej przy krawędzi, więc była pułapką bez wyjścia. Do tego pole grawitacyjne jest zachowawcze, więc wyjście z niego nie daje „darmowego” dystansu.
* **Decyzja / podejście:** czyste przyciąganie radialne, stałe w pierścieniach ustawionych między ciągiem a boostem statku. Wyrzut to impuls zależny od głębokości zejścia plus ok. 3 s niskiego oporu. Starą wersję zachowano jako `BlackHoleDeathTrap`.
* **Dlaczego:** przy oporze silników sam impuls wygasa po ok. 2 s i daje znikomy dystans. Odrzucono darmowy boost bez paliwa, bo nie daje odczuwalnego kopnięcia.
* **Powiązane:** #71, GAMEPLAY 4.5, `GravityAnomaly.cs`, `Player_Spaceship.Slingshot`.

## Proca grawitacyjna i efekt Obertha zamiast sztucznego wyrzutu, feat/gravity-anomaly-rings, 2026-10-14
* **Problem / kontekst:** pierwszy prototyp dawał impuls przy wyjściu z pierścienia wewnętrznego. W przelocie było to nienaturalne: opór silników (0.5) działał także w anomalii, więc spadanie ku jądru nie rozpędzało statku. Ucieczka była powolną wspinaczką (boost 24 minus przyciąganie 18), a na granicy statek nagle wystrzeliwał w kierunku celowania. Nagroda nie wynikała z tego, jak gracz leciał.
* **Decyzja / podejście:** wyrzut wynika z mechaniki orbitalnej, a nie z dopisanego impulsu. W całej strefie statek ma opór jak przy pustym baku, więc pole działa jak prawdziwe pole zachowawcze: statek, który wleci po stycznej, rozpędza się przy jądrze, zakręca i wylatuje. Przewagę daje **efekt Obertha**: praca siły ciągu to F·v, więc ten sam boost odpalony przy największej prędkości (w najniższym punkcie przelotu) dodaje więcej energii kinetycznej niż w pustej przestrzeni. Przy obecnych wartościach (krawędź 6 m/s² na 120 m, pierścień 18 m/s² na 130 m) statek wlatujący z 24 m/s ma przy jądrze ok. 80 m/s. 2 s boostu w tym punkcie dają na wylocie ok. 104 m/s, a te same 2 s w pustej przestrzeni – 72 m/s. Po wylocie ze strefy zostaje ok. 3 s niskiego oporu, żeby nadwyżka prędkości zamieniła się w dystans, a kamera kopie proporcjonalnie do prędkości wylotu.
* **Przełożenie na grę:** gracz uczy się trzech rzeczy: wchodzić po stycznej zamiast prosto w jądro, schodzić możliwie nisko bez wpadania w jądro i odpalać boost w najniższym punkcie. Pierścienie przestają oznaczać „tu nie uciekniesz” i zaczynają oznaczać „tu nie zatrzymasz się bez boostu” – ucieczka zależy od pędu, a nie tylko od siły ciągu. Głębokość sygnalizuje wibracja kamery i ciemniejące krawędzie ekranu, a dysk akrecyjny pokazuje płaszczyznę, w której opłaca się robić przelot (i w której krąży Rdzeń Stazy).
* **Dlaczego:** nagroda za umiejętność pilotażu jest czytelniejsza i bardziej satysfakcjonująca niż nagroda za samo przekroczenie linii. Rozwiązanie opiera się na prawdziwej fizyce (pole zachowawcze, efekt Obertha), więc jest naturalne w odczuciu i dobrze opisuje się w pracy. Odrzucono: impuls na granicy pierścienia (pierwszy prototyp – oderwany od akcji) i darmowy boost bez paliwa (brak odczuwalnego kopnięcia). Ryzyko do sprawdzenia w playteście: bez oporu w strefie zwykły ciąg też rozpędza statek bez limitu, więc strefę może się opłacać przelatywać „na skróty” – wtedy opór w strefie stanie się osobnym parametrem.
* **Powiązane:** #71, GAMEPLAY 4.5, `GravityAnomaly.cs`, `Player_Spaceship`.

## Sterowność w anomalii i widoczność rdzenia, feat/gravity-anomaly-rings, 2026-10-14
* **Problem / kontekst:** bez oporu w strefie statek przy dużej prędkości „wyślizgiwał się”, bo ciąg prawie nie zmieniał kierunku lotu. Rdzeń Stazy ginął wśród świecących cząsteczek dysku.
* **Decyzja / podejście:** asysta przy ciągu obraca wektor prędkości w stronę nosa bez zmiany jego długości, a opór włącza się dopiero powyżej ok. 110 m/s (miękki limit). Hologramowa linia pokazuje przewidywany tor lotu bez ciągu. Rdzeń wyróżnia się kontrastem: pionowy snop prostopadły do dysku i zimny kolor.
* **Dlaczego:** opór rosnący z prędkością hamowałby najmocniej przy jądrze, gdzie proca daje zysk (efekt Obertha), więc został tylko bezpiecznikiem. Hologramowe strzałki odrzucono, bo pokazywałyby gotową trasę zamiast skutku lotu i zabierałyby umiejętność znalezienia linii procy. Więcej cząsteczek wokół rdzenia dodałoby szumu, a widoczność daje kontrast kształtu i koloru; ciemny pas w dysku odrzucono.
* **Powiązane:** #71, GAMEPLAY 4.3 i 4.5, `Player_Spaceship`, `GravityAnomalyTrajectory.cs`, `StasisCore.prefab`.
