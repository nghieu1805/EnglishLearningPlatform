# Verification checklist

## Automated tests supplied (not executed in the generation environment)

```
dotnet test EnglishLearningPlatform.sln
```

QuizScoringTests covers mixed answers, unanswered questions, foreign answer IDs, unknown question IDs, ambiguous correct answers, empty quizzes, and historical text snapshots.

## Manual local integration checks

- Run setup against an empty EnglishLearningDb. Run setup again: no duplicate grade/demo topic, same migration history.
- Register a student; invalid/duplicate email and mismatched password must fail. Log in/out and edit profile.
- Anonymous user cannot POST progress or open Quiz; student cannot open Admin (403).
- POST without antiforgery token must fail; logout is POST only.
- User A's attempt result must return 404 to User B. Progress/dashboard must show only the signed-in user's records.
- Submitted points/UserId must have no influence on scoring/ownership. Tampered answer ID from another question must be rejected.
- Create draft topic and sources; reject mismatched vocabulary source; reject publishing official content without sources; verify draft does not appear in dictionary or lessons.
- Grade/topic deletion with referenced content must fail gracefully; deleting examples/audio must work; quiz results retain snapshot text after question edits.
- Quiz with no questions, fewer than two answers, or no correct answer must not open.
- Mark words learned, change to NeedsReview, reload Dashboard, restart server and verify persistence.
- Search dictionary-only sustainability; unknown word has an honest empty state.
- Keyboard navigation, small screen layout, form labels, show-meaning flashcards, audio fallback with/without available voices.

No HTTP/MySQL/browser integration checks above have been executed in the generation environment. Do not treat this checklist as a passing test report.
