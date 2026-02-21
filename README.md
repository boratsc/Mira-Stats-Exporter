# 🎮 Mira Stats Exporter

Plugin do Among Us, który zapisuje statystyki rozgrywek z moda **Town of Us: Mira** i pozwala je wykorzystać dalej — lokalnie, w panelu albo na Discordzie.

---

## 🧩 Co to jest

Mira Stats Exporter to dodatek do gry, który po każdej rundzie zbiera dane o meczu.

Nie zmienia moda.  
Działa obok niego.

Plik pluginu:

MiraStatsExporter.dll

---

## 📊 Co zbiera

Po zakończeniu gry zapisuje m.in.:

- role graczy (i zmiany ról)
- statystyki zabójstw
- taski i postęp
- wynik gry i zwycięzców
- modyfikatory postaci
- podstawowe dane lobby

Czyli pełne statystyki gier w Among Us.

---

## 🚀 Do czego to jest

Najczęściej używane do:

- statystyk gier w Among Us  
- statystyk serwera Among Us  
- paneli społeczności  
- rankingów graczy  
- integracji z Discordem  
- logów rozgrywek  
- backendu pod boty  

Plugin sam zbiera dane — a co z nimi zrobisz, zależy od Ciebie.

---

## ⚙️ Instalacja

1. Pobierz `MiraStatsExporter.dll`
2. Skopiuj do:

```
Among Us/BepInEx/plugins/
```

3. Uruchom grę raz (powstanie konfiguracja)
4. Ustaw konfigurację
5. Uruchom grę ponownie

Gotowe.

---

## 🛠️ Konfiguracja (prosto)

Plugin tworzy plik `ApiSet.ini`.

Możesz ustawić:

- czy eksport jest włączony  
- endpoint API  
- token  
- zapis lokalny  

Najprościej: włącz zapis lokalny i masz historię gier.

---

## 💾 Gdzie są statystyki

Jeśli zapis lokalny jest włączony:

```
Documents/TownOfUs/GameLogs/
```

Każda gra to osobny plik JSON.

---

## 🧰 SUSModder (najprostszy sposób)

Nie musisz instalować ręcznie.

Plugin jest wbudowany w:  
👉 https://github.com/boratsc/SUSModder

SUSModder ogarnia instalację modów i dodatków.

---

## 🤖 Integracja z Discordem (Clair)

Jeżeli chcesz wykorzystać statystyki na serwerze Discord (np. profile, systemy społeczności, automatyzacje), konfigurujesz eksport razem z:

👉 https://clairbot.app

Clair wykorzystuje dane z gier Among Us po odpowiedniej konfiguracji.

---

## ✅ Najważniejsze rzeczy

- działa z modem Town of Us: Mira  
- nie modyfikuje moda  
- można usunąć w dowolnym momencie  
- zbiera pełne statystyki gier w Among Us  
- nie wpływa zauważalnie na wydajność  

---

## 🧠 Typowy setup

Najczęściej wygląda to tak:

SUSModder → instalacja  
Mira Stats Exporter → dane  
Clair → funkcje na Discordzie  

---

## 📌 Kiedy tego używać

Używaj jeśli:

- prowadzisz serwer Among Us  
- budujesz społeczność  
- robisz statystyki  
- tworzysz bota  
- robisz panel  
- chcesz historię gier  

---

## TL;DR

Plugin od statystyk do Among Us (Town of Us: Mira).  
Zbiera dane z gier i pozwala wykorzystać je lokalnie albo w integracjach jak Clair.
