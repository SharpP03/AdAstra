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
