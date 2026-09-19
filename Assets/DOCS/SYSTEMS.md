# SYSTEMS.MD - AD ASTRA SYSTEM REGISTRY

Rejestr podsystemów projektu AdAstra. Służy jako operacyjna tablica stanu dla inżynierów oraz modeli AI. Każda modyfikacja podsystemu wymaga aktualizacji odpowiedniego wpisu w tym dokumencie.

---

## Statusy Podsystemów

| Status | Znaczenie |
| :--- | :--- |
| **`ZAIMPLEMENTOWANY`** | System działa w projekcie, ale może wymagać dopracowania lub optymalizacji. |
| **`CZĘŚCIOWO ZAIMPLEMENTOWANY`** | Część systemu istnieje w kodzie/scenie, lecz brakuje integracji, występują błędy lub luki w logice. |
| **`OPISANY / NIEZAIMPLEMENTOWANY`** | Koncepcja jest znana i pożądana, ale fizyczna implementacja nie istnieje w repozytorium (lub została wycięta). |
| **`NIEOPISANY`** | Element jest niezbędny dla pętli rozgrywki MVP, lecz brak jakichkolwiek reguł i założeń projektowych. |
| **`DO WERYFIKACJI`** | Otwarta kwestia architektoniczna lub projektowa wymagająca decyzji. |

---

## Rejestr Podsystemów

### 1. Sterowanie i Fizyka Statku (Spaceship Controller)
- **Status:** `CZĘŚCIOWO ZAIMPLEMENTOWANY`
- **Ścieżka:** `Assets/Scripts/Player/Spaceship/Player_Spaceship.cs`
- **Opis:** Model lotu 6DoF / Arcade oparty na `Rigidbody` i New Input Systemie. Obsługuje ruch postępowy, zwroty, obrót myszą (pitch/yaw), przechył (roll) oraz tryb sprintu.
- **Działania do podjęcia:**
  - [ ] Naprawić błąd logiczny: właściwość `MoveInput` w `Player_Spaceship.cs` nigdy nie jest aktualizowana (wartość `(0, 0)`), co powoduje, że tilt kamery jest martwy.
  - [ ] Przenieść odczyt wejścia myszy z `FixedUpdate()` do `Update()`, eliminując micro-stuttering i gubienie próbek wejścia.
  - [ ] Zastąpić wyszukiwanie stringowe akcji (`actions.FindAction("...")`) silnie typowanym wrapperem C#.

---

### 2. Zasób: Paliwo (Fuel System)
- **Status:** `ZAIMPLEMENTOWANY`
- **Ścieżka:** `Assets/Scripts/Player/Spaceship/FuelSystem.cs`
- **Opis:** Zarządza limitem i drenażem paliwa. Ruch statku i sprint zużywają paliwo w zróżnicowanym tempie. Wyczerpanie paliwa uniemożliwia sterowanie. Obsługuje uzupełnianie w strefach odnowienia.
- **Działania do podjęcia:**
  - [ ] Usunąć coroutine odpytującą `WaitForPlayer()` na rzecz bezpośredniej referencji lub inicjalizacji w `Awake`.
  - [ ] Usunąć tymczasowy debugowy skrót klawiszowy (`Keyboard.current.digit1Key`).

---

### 3. Zasób: Zdrowie i System Obrażeń (Health & Damage System)
- **Status:** `CZĘŚCIOWO ZAIMPLEMENTOWANY`
- **Ścieżka:** `Assets/Scripts/Player/Spaceship/HealthSystem.cs`, `Assets/Scripts/Interfaces/IDamageable.cs`
- **Opis:** Obsługuje punkty życia statku gracza przez interfejs `IDamageable`. Odbiera obrażenia z hazardów środowiskowych (np. burza jonowa).
- **Działania do podjęcia:**
  - [ ] Wprowadzić dolne clampowanie punktów życia (`Mathf.Max(0, ...)`), aby zdrowie nie spadało poniżej zera.
  - [ ] Zaimplementować reakcję na śmierć (event zniszczenia statku gracza / wywołanie Game Over).
  - [ ] Usunąć debugowy skrót klawisza `1` z `Update()`.

---

### 4. Kamera Dynamiczna (Dynamic Camera)
- **Status:** `CZĘŚCIOWO ZAIMPLEMENTOWANY`
- **Ścieżka:** `Assets/Scripts/Player/DynamicCamera.cs`
- **Opis:** Kamera TPP śledząca statek za pomocą `SmoothDamp`. Zwiększa FOV i oddala się podczas sprintu, reaguje na obrót w osi Z (roll) oraz nachylenie boczne (tilt).
- **Działania do podjęcia:**
  - [ ] Wyeliminować wyścig inicjalizacji w `Start()` (`GameManager.Instance.Player` może być `null` w momencie startu kamery, co całkowicie paraliżuje śledzenie).
  - [ ] Przywrócić działanie efektu `tilt` po naprawieniu właściwości `MoveInput` w graczu.

---

### 5. Efekty Wizualne Napędu (Thruster FX)
- **Status:** `ZAIMPLEMENTOWANY`
- **Ścieżka:** `Assets/Scripts/Player/Spaceship/PlayerThrusterFXController.cs`
- **Opis:** Płynnie interpoluje emisję, prędkość początkową, kolor oraz prędkość w osi Z cząsteczek napędu zależnie od stanu gracza (`Standstill`, `Moving`, `Sprinting`).
- **Działania do podjęcia:**
  - [ ] Usunąć pętlę oczekiwania `WaitForPlayer()` na rzecz bezpośredniej referencji lub rejestracji z poziomu obiektu gracza.

---

### 6. Interfejs Użytkownika / HUD (UI Manager)
- **Status:** `ZAIMPLEMENTOWANY`
- **Ścieżka:** `Assets/Scripts/Player/UI_Manager.cs`
- **Opis:** Prezentuje poziom paliwa i punktów życia za pomocą suwaków uGUI.
- **Działania do podjęcia:**
  - [ ] Zastąpić ciągłe odpytywanie wartości w `Update()` architekturą zdarzeniową (C# events wywoływane tylko przy zmianie wartości zasobu).
  - [ ] Zaprojektować minimalistyczne ekrany stanu gry (Ekran Końca Gry / Śmierci / Restart).

---

### 7. Stacje i Strefy Odnowienia (Refill Stations)
- **Status:** `CZĘŚCIOWO ZAIMPLEMENTOWANY`
- **Ścieżka:** `Assets/Scripts/Locations/StartingHub/`, `Assets/Scripts/Refill/`
- **Opis:** Komponent `RefuelZone` automatycznie wykrywa gracza przez trigger potomny (`RefuelChildCollider`) i uzupełnia wskazany zasób (`ResourceKind.Fuel` lub `ResourceKind.Health`), komunikując się z interfejsem `IRefillable`.
- **Działania do podjęcia:**
  - [ ] **Krytyczne:** Dodać komponenty `FuelRefillable` i `HealthRefillable` do prefabu `Player.prefab` (obecnie istnieją wyłącznie jako unapplied overrides na scenie).
  - [ ] Połączyć rozpakowaną geometrię stacji na scenie z prefabem `StartingHub.prefab`.

---

### 8. Zagrożenie: Anomalia Grawitacyjna (Gravity Anomaly)
- **Status:** `ZAIMPLEMENTOWANY`
- **Ścieżka:** `Assets/Scripts/Locations/GravityAnomaly/GravityAnomallyController.cs`, `Assets/Prefabs/Locations/GravityAnomaly/GravityAnomaly.prefab`
- **Opis:** Przyciąga obiekty z `Rigidbody`, nadaje siłę styczną tworząc stabilny ruch orbitalny, stosuje strefę surge na krawędzi oraz tłumi ucieczkę obiektów z orbity.
- **Działania do podjęcia:**
  - [ ] Poprawić literówkę w nazwie klasy (`GravityAnomallyController` -> `GravityAnomalyController`).
  - [ ] Rozważyć optymalizację `Physics.OverlapSphere` (np. bufor alokacji `OverlapSphereNonAlloc` lub trigger sferyczny).

---

### 9. Zagrożenie: Burza Jonowa / Mgła (Ion Storm Hazard)
- **Status:** `ZAIMPLEMENTOWANY`
- **Ścieżka:** `Assets/Scripts/Locations/IonStorm/FogEventController.cs`, `Assets/Prefabs/Locations/IonStorm/IonStorm.prefab`
- **Opis:** Obszar triggera uruchamiający cykliczną pętlę zagrożeń: generuje losowe punkty ostrzegawcze VFX, a po określonym opóźnieniu zadaje obrażenia przez `IDamageable`, weryfikując czy cel nie opuścił strefy rażenia.
- **Działania do podjęcia:**
  - [ ] Połączyć instancję strefy na scenie z prefabem `IonStorm.prefab`.

---

### 10. Generator Pola Asteroid (Asteroid Field Generator)
- **Status:** `ZAIMPLEMENTOWANY`
- **Ścieżka:** `Assets/Scripts/Locations/AsteroidField_01/AsteroidFieldGenerator.cs`, `Assets/Prefabs/Locations/AsteroidField/AsteroidField_01.prefab`
- **Opis:** Generuje w sferze o zadanym promieniu instancje asteroid z tablicy prefabów, nadając im losową skalę, collidery, komponenty `Rigidbody` oraz losowe impulsy siły i prędkości kątowej.
- **Działania do podjęcia:**
  - [ ] Ocenić wpływ na wydajność przy dużej liczbie obiektów (brak usuwania oddalonych asteroid, brak poolingu).
  - [ ] Ustalić rolę generatora w MVP (statyczna dekoracja startowa czy element procedury podróży).

---

### 11. Walka i Przeciwnicy (Combat & Enemy AI)
- **Status:** `OPISANY / NIEZAIMPLEMENTOWANY`
- **Ścieżka:** `Assets/Scripts/Enemy/EnemyController.cs`
- **Opis:** Poprzednia rozbudowana architektura AI została usunięta w commicie `98c75a8`. W projekcie pozostał jedynie pusty plik-szkielet `EnemyController.cs`, niepodpięty pod żaden obiekt.
- **Działania do podjęcia:**
  - [ ] W sesji `/grill-me` ustalić minimalistyczne założenia wroga w MVP (np. prosty dron śledzący / taranujący lub stacjonarna wieżyczka).
  - [ ] Zaimplementować prosty, bezpośredni skrypt bez zbędnych warstw abstrakcji (zgodnie z `AGENTS.md`).

---

### 12. Główna Pętla Gry i Cykl Życia (Game Loop & State Management)
- **Status:** `CZĘŚCIOWO ZAIMPLEMENTOWANY`
- **Ścieżka:** `Assets/Scripts/GameManagment/GameManager.cs`, `Assets/Scripts/GameManagment/GameState.cs`
- **Opis:** `GameManager` to obecnie minimalistyczny singleton z referencją do gracza. Enum `GameState` istnieje, ale nie jest używany. Brak pętli wygranej/przegranej.
- **Działania do podjęcia:**
  - [ ] Wdrożyć zarządzanie stanami gry (`MainMenu`, `Playing`, `Paused`, `GameOver`).
  - [ ] Zaimplementować procedurę restartu / resetu poziomu po zniszczeniu statku lub wyczerpaniu paliwa.

---

### 13. Struktura Przestrzeni i Cel Gry (World Flow & Objective)
- **Status:** `NIEOPISANY`
- **Ścieżka:** *Brak*
- **Opis:** Zgodnie z założeniami gra ma być minimalistycznym MVP o eksploracji i dotarciu do „końca”. Nie są ustalone reguły: czym jest „koniec” (lokacja docelowa, portal, punkt w przestrzeni?), jak gracz nawiguje (beacony radiowe, wskaźnik HUD, czy czysta eksploracja wzrokowa?).
- **Działania do podjęcia:**
  - [ ] Ustalić w sesji `/grill-me` docelowy warunek zwycięstwa oraz strukturę przestrzeni (jeden otwarty wycinek kosmosu ze stacjami i celem, czy podział na sektory).

---

### 14. Zapis Stanu i Trwałość Danych (Save / Persistence)
- **Status:** `DO WERYFIKACJI`
- **Ścieżka:** *Brak*
- **Opis:** Brak mechanizmu zapisu stanu. Dla krótkiego MVP (np. 5–10 minutowa sesja eksploracyjna) pełna serializacja stanu może być zbędna.
- **Działania do podjęcia:**
  - [ ] Zdecydować w sesji `/grill-me`, czy gra wymaga persystencji stanu, czy opiera się na formule pojedynczej sesji (permadeath / arcade run).
