# Release notes — Spotter 2.2.1

Paste into the GitHub release description. English first, Hebrew below, as GitHub renders both
in one body.

---

## 🔎 Spotter 2.2.1

One new thing, and a long list of corrections to 2.2.0.

### New

- **Filter the results by what kind of file they are.** **Files** is now a switch with a panel
  behind it: the switch decides whether files appear at all, and the chevron beside it opens
  eight buckets — images, video, audio, documents, archives, code and settings, programs and
  installers, everything else — plus a box for any extension the buckets do not cover. Hovering a
  bucket lists what is in it. The label carries the count while the filter is narrowed, so the
  row reads **Files (2)** rather than looking untouched while half the answers are missing.

  The custom extensions are an extra bucket rather than a restriction on the ticked ones: to find
  only `.iso`, untick the categories. The whole thing resets on every launch together with Files
  and Folders, under the same option in **Options → Searching**.

### Fixed

- **A second window no longer opens exactly on top of the first.** It steps clear by one title
  bar, keeps itself inside the work area, and never starts maximized.
- **A second window takes over when the first one closes.** Until now it stayed a second-class
  window for good: no restore hotkey, deaf to a re-launch, and silently writing no settings —
  while still holding the slot that stopped a fresh launch from working. It now picks the role up
  within a few seconds, keeping whatever the window that closed had written.
- **Saving Options or clearing the history in a second window says so** when the settings belong
  to the other window, instead of closing the dialog as though it had worked.
- **The refresh button no longer reshuffles the results.** Searches now have one order rather
  than one per run.
- **A "+" button beside minimize and close** opens a new window, and the title bar is shorter.
- **The size field's placeholder is readable again** — the name field gives it a quarter of its
  width.
- **Pasting a list of extensions works**, including one copied line-per-line, and anything that
  could never match a file name is dropped rather than counted as an active filter.
- **Dotfiles are found by what they are**: `.env`, `.env.local`, `.gitignore` and the rest sit
  under code and settings.
- **A control character pasted into a search no longer loses your settings.** One could stop
  every save for the rest of the session — window position, column widths, language, history —
  without a word.

### Also

Subtitles, macro-enabled Office documents, Matroska audio, internet shortcuts, split RAR volumes
and about forty other extensions the buckets were missing. Keyboard access to the file types
panel. Six languages throughout, as always.

---

<div dir="rtl">

## 🔎 ספוטר 2.2.1

דבר אחד חדש, ורשימה ארוכה של תיקונים ל‑2.2.0.

### חדש

- **סינון התוצאות לפי סוג הקובץ.** המתג **קבצים** הוא מעכשיו מתג עם חלונית מאחוריו: המתג קובע אם
  קבצים מופיעים בכלל, והחץ שלידו פותח שמונה סוגים — תמונות, וידאו, אודיו, מסמכים, ארכיונים, קוד
  והגדרות, תוכנות והתקנות, כל השאר — ובנוסף שדה לכל סיומת שהרשימה לא מכסה. ריחוף על סוג מציג מה יש
  בו. כשהסינון מצומצם התווית נושאת את המספר, כך שהשורה אומרת **קבצים (2)** במקום להיראות כאילו לא
  נגעו בה בזמן שחצי מהתשובות חסרות.

  הסיומות בשדה הן תוספת ולא הגבלה על המסומנים: כדי למצוא רק `.iso` יש לבטל את סימון הסוגים. הכול
  מתאפס בכל הפעלה יחד עם קבצים ותיקיות, תחת אותה הגדרה ב**אפשרויות ← חיפוש**.

### תוקן

- **חלון שני כבר לא נפתח בדיוק מעל הראשון.** הוא מתרחק בשורת כותרת אחת, נשאר בתוך אזור העבודה,
  ולא נפתח ממוקסם.
- **חלון שני מקבל את התפקיד כשהראשון נסגר.** עד עכשיו הוא נשאר חלון סוג ב׳ לתמיד: בלי קיצור
  שחזור, חירש להפעלה חוזרת, ובלי לכתוב הגדרות — בזמן שהוא עדיין מחזיק את הסלוט שמנע מהפעלה חדשה
  לעבוד. עכשיו הוא לוקח את התפקיד תוך כמה שניות, ושומר את מה שהחלון שנסגר הספיק לכתוב.
- **שמירה באפשרויות או ניקוי היסטוריה בחלון שני אומרים את זה** כשההגדרות שייכות לחלון האחר, במקום
  לסגור את הדיאלוג כאילו זה עבד.
- **כפתור הרענון כבר לא מערבב את התוצאות.** לחיפוש יש סדר אחד, ולא סדר לכל הרצה.
- **כפתור "+" לייד מזעור וסגירה** פותח חלון חדש, ושורת הכותרת נמוכה יותר.
- **ה‑placeholder של שדה הגודל קריא שוב** — שדה השם נותן לו רבע מהרוחב שלו.
- **הדבקת רשימת סיומות עובדת**, כולל כזו שהועתקה שורה‑בשורה, וכל מה שלא יכול להתאים לשם קובץ נזרק
  במקום להיספר כסינון פעיל.
- **קבצי נקודה נמצאים לפי מה שהם**: `.env`, `.env.local`, `.gitignore` וחבריהם יושבים תחת קוד
  והגדרות.
- **תו בקרה שהודבק לחיפוש כבר לא מאבד את ההגדרות.** אחד כזה יכול היה לעצור כל שמירה עד סוף הריצה
  — מיקום החלון, רוחבי העמודות, השפה, ההיסטוריה — בלי מילה.

### ועוד

כתוביות, מסמכי אופיס עם מאקרו, אודיו Matroska, קיצורי אינטרנט, כרכי RAR מפוצלים ועוד כארבעים
סיומות שחסרו ברשימות. גישה מהמקלדת לחלונית סוגי הקבצים. שש שפות לכל אורך הדרך, כרגיל.

</div>
