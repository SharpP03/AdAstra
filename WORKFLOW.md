# WORKFLOW.MD - PROCEDURY OPERACYJNE I PRZEPŁYW PRACY

Niniejszy dokument definiuje gotowe, techniczne procedury deweloperskie obowiązujące w projekcie **AdAstra**. Każdy workflow stanowi powtarzalny schemat działania gwarantujący spójność historii Gita, czystość architektury oraz pełną synchronizację z dokumentacją.

---

## 1. STANDARDOWY CYKL REALIZACJI ZADANIA (ISSUE -> PR)

Podstawowy przepływ pracy dla każdej nowej mechaniki, poprawki błędu, zmian w dokumentacji lub konfiguracji środowiska/CI.

### Krok 1: Definicja Taska (GitHub Issue)
Każda praca rozpoczyna się od utworzenia zadania w GitHub Issues.
- **Tytuł:** Krótki, precyzyjny, jednoznacznie określający cel zadania z prefiksem typu (`feat:`, `fix:`, `docs:`, `ci:`, `refactor:`).
- **Opis:** Zwięzłe nakreślenie kontekstu, celów oraz techniczna checklista podzadań (`- [ ]`).

**Szablon Issue:**
```markdown
## Cel
Krótkie wyjaśnienie (1-2 zdania), co ma zostać osiągnięte i dlaczego.

## Zakres prac
- [ ] Analiza istniejących skryptów/prefabów powiązanych z mechaniką
- [ ] Implementacja logiki w odrębnym module/asmdef
- [ ] Aktualizacja dokumentacji w Assets/DOCS/SYSTEMS.md
- [ ] Weryfikacja w Unity (0 błędów, 0 ostrzeżeń)
```

**Komenda CLI:**
```powershell
gh issue create --title "feat: system chłodzenia reaktora" --body-file issue_template.md
```

---

### Krok 2: Zaplanowanie pracy przed implementacją
Przed edycją kodu lub sceny należy przeprowadzić analizę:
1. **Zależności:** Sprawdź `Assets/DOCS/SYSTEMS.md` oraz powiązane pliki `.asmdef`.
2. **KISS & YAGNI:** Ustal minimalny zakres kodu spełniający zadanie bez nadmiarowych abstrakcji.
3. **Plan commitów:** Podziel pracę na logiczne, niezależne kroki (np. 1. Model danych / SO $\rightarrow$ 2. Logika fizyki/MonoBehaviour $\rightarrow$ 3. Wiring w prefabie $\rightarrow$ 4. Docs).

---

### Krok 3: Utworzenie dedykowanego brancha
Nigdy nie commitujemy bezpośrednio do `main`. Branch tworzony jest z najświeższego stanu gałęzi głównej.
- **Konwencja nazw:** `<kategoria>/<opis-kebab-case>`, np.:
  - `feat/reactor-cooling-system`
  - `fix/ship-drift-fixedupdate`
  - `docs/update-architecture`
  - `ci/auto-weekly-tag`

**Komendy CLI:**
```powershell
git checkout main
git pull
git checkout -b feat/reactor-cooling-system
```

---

### Krok 4: Wprowadzenie zmian w atomowych commitach
Zmiany dzielimy na małe, sensowne commity skupione wokół jednej odpowiedzialności.
- Każdy commit musi utrzymywać projekt w stanie kompilowalnym (0 błędów kompilacji).
- Stosuj konwencję **Conventional Commits**:
  - `feat: add CoolantTank ScriptableObject definition`
  - `feat: implement heat dissipation calculation in ReactorController`
  - `chore: update Player.prefab with CoolantTank serialized reference`
  - `docs: update SYSTEMS.md checklist for reactor cooling`
- **Zakaz śmieciowych plików:** Przed `git add` upewnij się, że pliki tymczasowe edytora są ignorowane przez `.gitignore`.

**Komendy CLI:**
```powershell
git add Assets/Scripts/Reactor/
git commit -m "feat: implement heat dissipation calculation in ReactorController"
```

---

### Krok 5: Otwarcie Pull Requesta (PR)
Gdy implementacja jest gotowa i spełnia założenia checklisty z Issue:
1. Wypchnij branch na zdalne repozytorium (`origin`).
2. Otwórz PR powiązany z numerem zadania za pomocą słowa kluczowego `Closes #<ID>` (automatyczne zamykanie zadania przy scaleniu).
3. Dołącz zwięzłe podsumowanie zmian dla celów przeglądu.

**Komenda CLI:**
```powershell
git push -u origin feat/reactor-cooling-system
gh pr create --title "feat: reactor cooling system" --body "Implements reactor heat dissipation loop.`n`nCloses #12"
```

---

### Krok 6: Weryfikacja (Definition of Done) i Merge
Przed scaleniem PR-a upewnij się, że:
1. Zmiany kompilują się bez błędów w Unity 6 (`6000.2.6f2`).
2. Prefaby posiadają kompletne referencje (brak `Missing (MonoBehaviour)`).
3. Wszelkie uwagi do pracy inżynierskiej (`DO PRACY INŻ.`) zostały zachowane.
4. Zaktualizowano checklistę w `Assets/DOCS/SYSTEMS.md`.
5. Po weryfikacji następuje scalenie do `main` (Merge commit lub Squash and merge):
   ```powershell
   gh pr merge --merge --delete-branch
   ```
