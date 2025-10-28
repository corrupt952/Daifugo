# Daifugo - Design Decisions and Clarifications

## Purpose

This document records important design decisions, technical choices, and specification clarifications for the Daifugo project. Each clarification follows a structured format to ensure clear communication of context, decisions, rationale, impact, and alternatives.

## Checklist

- [ ] Document all major technical decisions
- [ ] Follow the clarification format (Date, Status, Context, Decision, Rationale, Impact, Alternatives)
- [ ] Update this document when new design decisions are made
- [ ] Include reference links for external resources

---

## Clarification Format

Each clarification follows this structure:

```markdown
## [YYYY-MM-DD] Title

**Status**: Approved / Under Discussion / On Hold

**Context**:
Why this decision was needed

**Decision**:
What was decided

**Rationale**:
Why this decision was made

**Impact**:
Effects of this decision

**Alternatives**:
Other options that were considered
```

---

## [2025-10-28] Project Initialization

**Status**: Approved

**Context**:
The Daifugo project requires initial setup, including SpecKit workflow, coding standards, and documentation structure.

**Decision**:
- Inherit architecture patterns from the Rookie project
- Adopt SpecKit (Spec-Driven Development)
- Apply ScriptableObject + EventChannel + RuntimeSet patterns
- Use phased development approach: 2D (UI Toolkit) → 3D

**Rationale**:
1. **Proven patterns**: Design patterns established in Rookie have proven track record
2. **Learning efficiency**: Using familiar patterns allows focus on implementation
3. **Gradual learning**: 2D → 3D phased approach provides gentle learning curve
4. **Reusability**: Phase 1 logic can be reused in Phase 2

**Impact**:
- Documentation structure maintains consistency with Rookie
- ScriptableObject-based design is required
- Event-driven architecture is required
- Tang3cko.EventChannels package is required

**Alternatives**:
1. MonoBehaviour-centric design → Rejected (lower decoupling)
2. Start with 3D development → Rejected (steep learning curve)
3. Custom event system → Rejected (reinventing the wheel)

---

## [2025-10-29] Phase 2 Animation Library Selection

**Status**: Approved

**Context**:
Phase 2 (3D version) requires card animation implementation. While DOTween was initially planned, research into 2024-2025 trends revealed higher-performance alternatives.

**Decision**:
- Adopt **LitMotion** as the animation library for Phase 2
- Use LitMotion instead of DOTween

**Rationale**:
1. **Best performance**: Approximately 5x faster than DOTween, zero allocation
2. **Latest technology**: Leverages Unity DOTS (C# Job System + Burst Compiler)
3. **Rich features**: v2 supports Sequence functionality and Inspector editing
4. **Learning value**: Opportunity to learn latest Unity technology (DOTS)
5. **Japanese documentation**: Author is Japanese with complete Japanese documentation

**Impact**:
- LitMotion integration required for Phase 2 implementation
- Performance improvements enable potential mobile deployment
- Learning opportunity for DOTS technology

**Alternatives**:
1. **DOTween** → Rejected (proven but inferior performance)
2. **PrimeTween** → Considered but LitMotion offers better performance
3. **MagicTween** → Rejected (LitMotion is improved successor)

**Reference Links**:
- LitMotion GitHub: https://github.com/AnnulusGames/LitMotion
- Performance Comparison: https://github.com/AnnulusGames/TweenPerformance

---

## Future Records

When new design decisions or specification clarifications are needed, add them to this document. The more important the decision, the more detail should be recorded.

---

## References

- [SpecKit - Spec-Driven Development](https://github.com/github/spec-kit)
- [Coding Standards](../03_technical/coding_standards.md)
- [Project Overview](../01_overview/README.md)
- [LitMotion GitHub](https://github.com/AnnulusGames/LitMotion)
