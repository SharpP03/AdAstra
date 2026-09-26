# TODO - PLAN DZIAŁANIA I POPRAWEK AD ASTRA

Plan techniczny sporządzony na podstawie audytu kodu, wytycznych Gamedev Skills (`physics-tuning`, `input-systems`, `unity-physics`, `unity-input-system`) oraz założeń projektowych Deep Modules (Matt Pocock / *Codebase Design*).

---

## Faza 1: Konieczne poprawki fundamentów (Pilne / Blockery)

Zadania usuwające błędy logiczne, race conditions i naruszenia integralności prefabów przed rozbudową pętli gry.

- [ ] **Integralność Prefabu Gracza (`Player.prefab`)**
  - Dodać komponenty `FuelRefillable` oraz `HealthRefillable` bezpośrednio do `Assets/Prefabs/Player/Player.prefab`.
  - Usunąć niezatwierdzone lokalne override'y z `SampleScene.unity`.
- [ ] **Błąd logiki wejścia i kamery (`Player_Spaceship.cs` & `DynamicCamera.cs`)**
  - Przypisywać publiczną właściwość `MoveInput` w `Player_Spaceship.Update()` (usunięcie lokalnego cienia zmiennej).
  - Normalizować / zacisnąć wektor ruchu (`Vector2.ClampMagnitude(..., 1f)`), zapobiegając poruszaniu się po skosie o ~41% szybciej.
- [ ] **Poprawny timing wejścia i fizyki (`Player_Spaceship.cs`)**
  - Odczyt wejść ciągłych (klawiatura, ruch myszy) przenieść do `Update()` i buforować w polach prywatnych.
  - Usunąć mnożenie delty myszy przez `Time.deltaTime` wewnątrz `FixedUpdate()` (wyeliminowanie jittera przy zmiennym framerate).
  - W `FixedUpdate()` aplikować wyłącznie siły i momenty obrotowe (`AddForce`, `AddTorque`).
- [ ] **Eliminacja wyścigu inicjalizacji (`DynamicCamera.cs`)**
  - Zastąpić zależność `GameManager.Instance.Player` pobieraniem komponentu bezpośrednio ze wskazanego `target` (`target.GetComponentInParent<Player_Spaceship>()`) w `Awake()`/`Start()`.
- [ ] **Usunięcie antywzorca pollingu coroutine (`FuelSystem.cs` & `PlayerThrusterFXController.cs`)**
  - Zastąpić pętle `while (GameManager.Instance == null || ...)` bezpośrednim `GetComponent<Player_Spaceship>()` w `Awake()`.
- [ ] **Integralność i odporność modułu zdrowia (`HealthSystem.cs`)**
  - Dodać dolne ograniczenie zdrowia (`Mathf.Max(0f, ...)`) w `TakeDamage`.
  - Usunąć redundantną metodę `PlayerTakeDamage` (płytki wrapper, łamiący zasadę *deletion test*).
  - Wprowadzić zdarzenia `event Action<float, float> OnHealthChanged` oraz `event Action OnDied`.

---

## Faza 2: Głębokie moduły i architektura zdarzeń (Codebase Design)

Zmniejszenie powierzchni interfejsów (leverage), eliminacja zależności cyklicznych i przejście na zdarzeniowe UI.

- [ ] **Samodzielny i głęboki moduł paliwa (`FuelSystem.cs`)**
  - Odciąć zależność `FuelSystem` od `PlayerState` i `GameManager`.
  - Wystawić czysty interfejs zarządzania bakiem: `bool TryConsume(float amount)`, `void AddFuel(float amount)`, właściwości odczytowe oraz zdarzenie `event Action<float, float> OnFuelChanged`.
  - Zużycie paliwa wywoływane w `Player_Spaceship` wyłącznie wtedy, gdy statek faktycznie generuje ciąg.
- [ ] **Event-driven HUD (`UI_Manager.cs`)**
  - Zastąpić co-klatkowe odpytywanie wartości w `Update()` subskrypcją zdarzeń `OnFuelChanged` oraz `OnHealthChanged` w `OnEnable()`/`OnDisable()`.
- [ ] **Silnie typowane wrappery wejścia New Input System**
  - Wygenerować klasę C# z assetu `playerInput.inputactions`.
  - Zastąpić wyszukiwanie stringowe (`actions.FindAction(...)`) silnie typowanymi odwołaniami.

---

## Faza 3: Domknięcie rdzennej pętli rozgrywki (Core Loop MVP)

- [ ] **Stan Dead-Stick przy wyczerpaniu paliwa**
  - Przy `Fuel == 0`: odcięcie napędu głównego i manewrowego RCS w `Player_Spaceship`. Statek zachowuje fizyczną bezwładność pędu, nie wywołując natychmiastowego Game Over.
- [ ] **Pętla życia gry w `GameManager.cs`**
  - Implementacja stanów `GameState` (`Playing`, `GameOver`, `Victory`).
  - Reakcja na `HealthSystem.OnDied` -> przejście do `GameOver` i wyświetlenie ekranu z przyciskiem restartu sceny.
- [ ] **Stacja dokująca z mechaniką Dead-Stop & Magnes Dokujący (`RefuelZone.cs`)**
  - Przebudowa natychmiastowego tankowania przy `OnTriggerEnter` na wymóg wyhamowania poniżej prędkości progowej.
  - Magnes dokujący stabilizujący statek w strefie na czas stopniowego uzupełniania surowca.

---

## Faza 4: Dynamika świata i presja czasu (Korytarz Z=0..4000)

- [ ] **Front Anihilacji (`AnnihilationWave`)**
  - Płaszczyzna przesuwająca się ze stałą prędkością po osi Z za graczem.
  - Detekcja odległości, ostrzeżenie na HUD w strefie zbliżenia (50-100m).
  - Zadawanie obrażeń Rapid DPS przez `IDamageable` przy kontakcie fizycznym.
- [ ] **Brama Warp Gate (Z=4000)**
  - Trigger ekstrakcji wywołujący stan `GameState.Victory`.
- [ ] **Wskaźniki celów (HUD Waypoints)**
  - Kierunek i dystans w metrach do stacji pośredniej i Warp Gate.
