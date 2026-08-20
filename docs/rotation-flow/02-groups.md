# 02 · Ablaufstruktur je Jobgruppe

Aufbauend auf `01-jobs.md`. Diese Ebene zeigt pro Gruppe, was **gleich**,
**ähnlich** und **unterschiedlich** ist. Farbcode in allen Diagrammen:

- durchgezogen = bei jedem Job der Gruppe vorhanden
- gestrichelt = bei einigen Jobs der Gruppe vorhanden
- Raute = Verzweigung, an der sich die Jobs unterscheiden

---

## Heiler (WHM · AST · SGE · SCH)

```mermaid
flowchart TD
    A[GeneralGCD] --> B{Raise ansteht<br/>und Swiftcast bereit?}
    B -- ja --> C[Kurzschluss: base / RaiseGCD]
    B -- nein --> D{Sustain-Zweig<br/>vorhanden?}
    D -- "WHM AST SGE" --> E[TrySustain…OnTank]
    D -- "SCH" --> F[kein Sustain]
    E --> G[Job-Ressource]
    F --> G
    G --> H[AoE-Zweig]
    H --> I[DoT-Kette]
    I --> J[Filler-Level-Kette]
    J --> K[base.GeneralGCD]

    style C fill:#4a3,color:#fff
    style E fill:#36c,color:#fff
    style F fill:#a33,color:#fff
```

### Identisch in allen vier

1. **Raise-Kurzschluss ganz oben.** Wortgleich, 13 Kopien über die vier
   Dateien (jeder Heiler hat ihn zusätzlich in `HealSingleGCD` und
   `HealAreaGCD`). Der einzige Zweig, der die Methode verlässt, bevor
   irgendeine Rotationsentscheidung fällt.
2. **Reihenfolge AoE → DoT → Filler.** Ohne Ausnahme.
3. **Filler ist immer eine Level-Kette** (Glare/Malefic/Dosis/Broil).
4. **`CanHealSingleSpell`/`CanHealAreaSpell`** mit identischem Ausdruck
   `base && (GCDHeal || aliveHealerCount == 1)` — vier wortgleiche Kopien.

### Ähnlich, aber abweichend

| | WHM | AST | SGE | SCH |
|---|---|---|---|---|
| Kurzschlüsse | **2** (ThinAir + Swift) | 1 | 1 | 1 |
| Sustain-Zweig | Regen | Aspected Benefic | Eukr. Diagnosis | **keiner** |
| HP-Boden Sustain | 0.3 | 0.4 | **keiner** | – |
| `GCDHeal`-Default | **true** | false | false | false |
| Ressourcenlogik in GeneralGCD | Lily | **keine** | Phlegma | MP-Schwelle |
| Zweistufiger Cast | nein | nein | **ja** (Eukrasia) | nein |
| Pet-Verwaltung | nein | nein | nein | **ja** (Eos) |

### Wo die Gruppe wirklich auseinanderläuft

Nur an drei Stellen: **Eukrasia-Vorstufe** (SGE), **Pet** (SCH) und
**Kartenlogik** (AST, komplett außerhalb `GeneralGCD`). Alles andere ist
dieselbe Leiter mit anderen Aktionsnamen.

---

## Tanks (PLD · WAR · DRK · GNB)

```mermaid
flowchart TD
    A[GeneralGCD] --> B[Ressource: Kartuschen / Blut / Beast Gauge]
    B --> C{Gegneranzahl}
    C -- "≥ AoeCount" --> D[AoE-Combo]
    C -- "sonst" --> E[ST-Combo]
    D --> F[Ranged-Pull-Fallback]
    E --> F
    F --> G[base.GeneralGCD]

    H[DefenseAreaAbility] -.-> I{HasHostileCountAoeMitigation}
    I -- "DRK GNB" --> J[Reprisal-Sustain]
    I -- "PLD WAR" --> K[nur reaktiv]

    style J fill:#36c,color:#fff
    style K fill:#a33,color:#fff
```

### Identisch in allen vier

1. **Ressource → AoE/ST-Verzweigung → Combo → Ranged-Fallback.** Gleiche
   Makrostruktur, gleiche Reihenfolge.
2. **Ranged-Pull-Fallback am Ende von `GeneralGCD`**, direkt vor `base`, nur
   durch das eigene `CanUse` gegated: Tomahawk (WAR), Lightning Shot (GNB),
   Shield Lob (PLD), Unmend (DRK). Vier strukturgleiche Zeilen.
3. **`AoeCount = 2`** für die AoE-Aggro-Aktion — bei allen vier explizit
   überschrieben (globaler Default wäre 3).
4. **Keine GCD-Heilung, kein Raise** (Ausnahme PLD/WAR `HealSingleGCD`).

### Wo die Gruppe auseinanderläuft

| | PLD | WAR | DRK | GNB |
|---|---|---|---|---|
| `EmergencyAbility` | ✓ | **fehlt** | ✓ | ✓ |
| `HealSingleGCD` | ✓ Clemency | ✓ | – | – |
| `HasHostileCountAoeMitigation` | **fehlt** | **fehlt** | ✓ | ✓ |
| Reprisal-Sustain in | nur DefenseSingle | nur DefenseSingle | **Area + Single** | **Area + Single** |
| Configs | 15 | 12 | 8 | **2** |

Die PLD/WAR-vs-DRK/GNB-Spaltung bei Reprisal folgt der Upstream-Platzierung
(PLD/WAR haben Reprisal nur in `DefenseSingleAbility`) — sie ist damit
begründet, aber sie macht die Gruppe an einer fachlich einheitlichen Stelle
uneinheitlich.

---

## Melee (DRG · MNK · NIN · RPR · SAM · VPR)

```mermaid
flowchart TD
    A[GeneralGCD] --> B{Combo-Ablauf<br/>gefährdet?}
    B -- "RPR" --> C[Combo-Rettung]
    B -- nein --> D[Burst-Fenster]
    C --> D
    D --> E{Combo-Darstellung}
    E -- "DRG SAM NIN RPR" --> F[if-Kette]
    E -- "VPR" --> G[switch über Status-Tupel]
    E -- "MNK" --> H[Formen-Automat]
    F --> I[AoE-Zweig]
    G --> I
    H --> I
    I --> J[Ranged-Fallback]
    J --> K[base.GeneralGCD]

    style G fill:#4a3,color:#fff
    style H fill:#4a3,color:#fff
```

### Identisch in allen sechs

1. **Feint-Sustain** über `ShouldSustainMitigationDebuff(StatusID.Feint)` in
   `DefenseAreaAbility` **und** `DefenseSingleAbility` — nach der
   Vereinheitlichung eine Zeile pro Stelle, zwölf Stellen gesamt.
2. **Ranged-Fallback am Ende** (Piercing Talon, Writhing Snap, Harpe …).
3. **`HasHostileCountAoeMitigation = true`** bei allen sechs.
4. **`HealSingleAbility`** (Second Wind/Bloodbath) — außer MNK.

### Wo die Gruppe auseinanderläuft

| | DRG | MNK | NIN | RPR | SAM | VPR |
|---|---|---|---|---|---|---|
| `CountDownAction` | **fehlt** | ✓ | ✓ | ✓ | ✓ | **fehlt** |
| `EmergencyAbility` | ✓ | ✓ | ✓ | **fehlt** | **fehlt** | ✓ |
| `HealSingleAbility` | ✓ | **fehlt** | ✓ | ✓ | ✓ | ✓ |
| `HealAreaAbility` | – | **✓** | – | – | – | – |
| `HasOwnInterruptGate` | – | – | – | ✓ | – | ✓ |
| Combo-Form | if-Kette | Automat | Automat | if-Kette | if-Kette | switch |
| Trait-Duplikate | **8 Paare** | – | – | – | – | – |

MNKs `HealAreaAbility`-statt-`HealSingleAbility` ist die einzige Abweichung
ohne erkennbare fachliche Begründung.

---

## Physische Fernkämpfer (BRD · MCH · DNC)

```mermaid
flowchart TD
    A[GeneralGCD] --> B[DoT / Ressourcen-Erhalt]
    B --> C[Burst-Fenster]
    C --> D{Gegneranzahl}
    D -- AoE --> E[AoE-Level-Kette]
    D -- ST --> F[ST-Level-Kette]
    E --> G[base.GeneralGCD]
    F --> G

    H[Gruppen-Mitigation] -.-> I{welche Aktion}
    I -- BRD --> J[Troubadour]
    I -- MCH --> K[Tactician]
    I -- DNC --> L[Shield Samba]

    style J fill:#36c,color:#fff
    style K fill:#36c,color:#fff
    style L fill:#36c,color:#fff
```

### Identisch in allen drei

1. **Rollen-Mitigation als Selbstbuff** (Troubadour/Tactician/Shield Samba) —
   dieselbe Rolle, dieselbe Dauer (15 s), dieselbe BMR-Refresh-Logik.
2. **Level-Ketten dominieren den Filler.**
3. **Kein Raise, keine GCD-Heilung.**

### Wo die Gruppe auseinanderläuft

| | BRD | MCH | DNC |
|---|---|---|---|
| `DefenseSingleAbility` | ✓ | ✓ | **fehlt** |
| `HealAreaAbility` | – | – | **✓** |
| `HealSingleAbility` | ✓ | **fehlt** | ✓ |
| `DispelAbility` | **✓** | – | – |
| `MoveForwardAbility` | – | – | ✓ |
| Level-Ketten-Glieder | 2 | **12** | 0 |

Die Gruppe ist bei den Hooks am inkonsistentesten: keine zwei Jobs belegen
dieselbe Slot-Menge.

---

## Magische Fernkämpfer (SMN · RDM · PCT · BLM)

```mermaid
flowchart TD
    A[GeneralGCD] --> B{Phasen-/Zustandsautomat}
    B -- "BLM" --> C[InFireOrIce – ausgelagert]
    B -- "SMN" --> D[Bahamut / Phoenix / Solar]
    B -- "RDM" --> E[Mana-Balance]
    B -- "PCT" --> F[Motif / Muse]
    C --> G[Filler]
    D --> G
    E --> G
    F --> G
    G --> H[base.GeneralGCD]

    style C fill:#4a3,color:#fff
```

### Identisch in allen vier

1. **Addle-Sustain** über `ShouldSustainMitigationDebuff(StatusID.Addle)` in
   beiden Defense-Slots.
2. **`HasHostileCountAoeMitigation = true`.**
3. **Ein Zustandsautomat als Kern**, Filler nur als Rest.

### Wo die Gruppe auseinanderläuft

| | SMN | RDM | PCT | BLM |
|---|---|---|---|---|
| Automat liegt in | `GeneralGCD` | `GeneralGCD` | `GeneralGCD` | **privaten Methoden** |
| Top-Level-Zweige | 24 | 22 | 33 | **7** |
| `HealSingleGCD` | ✓ | ✓ | – | – |
| `RaiseGCD` | ✓ | ✓ | – | – |
| `MoveForwardGCD` | ✓ | – | – | – |
| Configs | 14 | 9 | 8 | 4 |

BLM erreicht mit **7** Top-Level-Zweigen dieselbe fachliche Abdeckung, für die
PCT **33** braucht. Der Unterschied ist reine Ablauforganisation, nicht
Job-Komplexität — das ist der stärkste Einzelbefund dieser Ebene.

---

## Gruppenvergleich in einer Tabelle

| Gruppe | gemeinsame Makrostruktur | echte Abweichungen | unbegründete Abweichungen |
|---|---|---|---|
| Heiler | Kurzschluss → Sustain → Ressource → AoE → DoT → Filler | Eukrasia, Pet, Karten | SCH ohne Sustain, HP-Boden 0.3/0.4/keiner |
| Tanks | Ressource → AoE/ST → Combo → Ranged-Fallback | – | WAR ohne Emergency, PLD/WAR ohne HCA-Flag |
| Melee | Burst → Combo → AoE → Ranged-Fallback | Mudra, Formen, Positionals | DRG/VPR ohne CountDown, MNK-Heal-Slot |
| Phys. Ranged | DoT/Ressource → Burst → AoE/ST | Tänze (DNC) | drei verschiedene Slot-Mengen |
| Mag. Ranged | Automat → Filler | Elementphasen | Automat mal ausgelagert, mal inline |
