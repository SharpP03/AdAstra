# WORKFLOW.MD - PROCEDURA WSPÓŁPRACY Z GITHUB

Niniejszy dokument definiuje procedurę postępowania przy realizacji nowych zadań (features / fixes) w projekcie **AdAstra** z wykorzystaniem serwisu GitHub oraz GitHub Projects.

---

## 1. Procedura: Nowy Feature / Zadanie

### Krok 1: Trigger inicjujący (Pytanie do Użytkownika)
Przed rozpoczęciem prac nad nowym zadaniem zapytaj użytkownika:
> **„Czy chcesz utworzyć nowy branch i task na GitHub Projects?”**

* **Jeśli odpowiedź brzmi NIE:**
  * Zignoruj całą procedurę opisaną poniżej w krokach 2–6.
  * Realizuj pracę bezpośrednio na bieżącej gałęzi lub według bieżących ustaleń na czacie.

* **Jeśli odpowiedź brzmi TAK:**
  * Przejdź do realizacji poniższych kroków (Kroki 2–6).

---

### Krok 2: Utworzenie taska w GitHub Projects
Utwórz nowy element (item) w projekcie za pomocą GitHub CLI:
* **Lokalizacja:** `https://github.com/users/SharpP03/projects/3/views/1`
* **Tytuł:** Krótki i zwięzły, jednoznacznie opisujący cel zadania.
* **Opis:** Szczegółowy opis zakresu prac, kontekst, cele i kryteria akceptacji zadania (cały opis merytoryczny zadania znajduje się bezpośrednio w tasku na boardzie).
* **Stopka (wymagana):** Na końcu opisu zadania umieść notatkę:
  ```text
  Created by <Model Name> on <GitHub Username>'s behalf
  ```
  *(np. `Created by Gemini 3.8 Flash on SharpP03's behalf`)*

**Komenda CLI:**
```powershell
gh project item-create 3 --owner SharpP03 --title "<Tytuł>" --body "<Opis>`n`nCreated by <Model Name> on SharpP03's behalf"
```

---

### Krok 3: Utworzenie dedykowanego brancha
Utwórz nowy branch z najnowszego stanu `main` o nazwie zgodnej z konwencją i przełącz się na niego:
* `feat/<opis-kebab-case>` – dla nowych mechanik / funkcjonalności
* `fix/<opis-kebab-case>` – dla poprawek błędów
* `refactor/<opis-kebab-case>` – dla refaktoryzacji kodu
* `docs/<opis-kebab-case>` – dla zmian w dokumentacji

**Komendy CLI:**
```powershell
git checkout main
git pull
git checkout -b <kategoria>/<opis-kebab-case>
```

---

### Krok 4: Realizacja prac i weryfikacja (Definition of Done)
1. Wykonuj zaplanowane prace na utworzonym branchu zgodnie z przebiegiem konwersacji i ustaleniami z użytkownikiem.
2. Twórz atomowe commity z czytelnymi komunikatami zgodnymi z Conventional Commits:
   ```powershell
   git add <ścieżki do zmodyfikowanych plików>
   git commit -m "feat: opis zrealizowanej części zadania"
   ```
3. **Weryfikacja jakości (Definition of Done) przed otwarciem PR:**
   * Kod musi kompilować się bez błędów w Unity 6 (`6000.2.6f2`).
   * Brak ostrzeżeń i błędów w konsoli Unity wywołanych zmianami.
   * Wszystkie zmodyfikowane prefaby i sceny mają kompletne referencje (brak `Missing (MonoBehaviour)` / null GUID).
   * Zaktualizowano checklistę i status mechaniki w `Assets/DOCS/SYSTEMS.md`.
   * Przejrzano `git diff` przed wysłaniem, upewniając się, że nie ma plików śmieciowych ani przypadkowych zmian.

---

### Krok 5: Otwarcie Pull Requesta (PR)
Po pomyślnej weryfikacji wypchnij branch i otwórz Pull Request:
1. **Wypchnięcie gałęzi na serwer:**
   ```powershell
   git push -u origin <nazwa-brancha>
   ```
2. **Utworzenie PR:**
   * **Tytuł PR:** Zgodny z wprowadzonymi zmianami (np. `feat: implement docking magnet mechanism`).
   * **Opis PR:** Zawiera wyłącznie referencję do powiązanego taska na boardzie (`ref: <link do taska>`). Nie powielamy szczegółowego opisu ani podsumowania zmian w PR – cała dokumentacja i opis zadania żyją w tasku na GitHub Projects:
     ```text
     ref: <link do utworzonego wcześniej taska>
     ```
     **Komenda CLI:**
     ```powershell
     gh pr create --title "<Tytuł PR>" --body "ref: <link do taska>"
     ```
3. **Powiadomienie użytkownika:** Poinformuj użytkownika na czacie o tytule utworzonego PR-a oraz przekaż link.

---

### Krok 6: Zakończenie prac i Squash Merge
Po wspólnym ustaleniu z użytkownikiem, że zadanie zostało w pełni ukończone:
1. Zapytaj użytkownika:
   > **„Czy chcesz wykonać squash merge do gałęzi main?”**
2. Jeśli użytkownik odpowie **TAK**:
   * Wykonaj operację **Squash and Merge**:
     ```powershell
     gh pr merge --squash
     ```
3. Po poprawnym scaleniu zapytaj użytkownika:
   > **„Czy chcesz usunąć gałąź roboczą <nazwa-brancha> (lokalnie i zdalnie)?”**
4. Jeśli użytkownik odpowie **TAK**:
   * Przełącz się na `main`, zaktualizuj stan repozytorium i usuń gałąź:
     ```powershell
     git checkout main
     git pull
     git branch -d <nazwa-brancha>
     git push origin --delete <nazwa-brancha>
     ```
