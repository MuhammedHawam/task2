using GAIA.Core.Assessment.Interfaces;
using GAIA.Core.InsightContent.Commands;
using GAIA.Core.InsightContent.Interfaces;
using GAIA.Domain.Assessment.DomainEvents;
using GAIA.Domain.Assessment.Entities;
using GAIA.Domain.InsightContent.Entities;
using Xunit;

namespace GAIA.Tests.Core.InsightContent;

public class UpdateInsightContentCommandHandlerTests
{
  [Fact]
  public async Task Handle_ReturnsNull_WhenAssessmentDoesNotExist()
  {
    var handler = new UpdateInsightContentCommandHandler(
      new FakeAssessmentRepository(),
      new FakeInsightContentRepository(),
      new FakeAssessmentEventWriter());

    var result = await handler.Handle(
      new UpdateInsightContentCommand(Guid.NewGuid(), Guid.NewGuid(), "content"),
      CancellationToken.None);

    Assert.Null(result);
  }

  [Fact]
  public async Task Handle_CreatesInsight_WhenMissing()
  {
    var assessmentId = Guid.NewGuid();
    var assessmentRepo = new FakeAssessmentRepository();
    assessmentRepo.Seed(new Assessment { Id = assessmentId, CreatedBy = Guid.NewGuid() });

    var insightRepo = new FakeInsightContentRepository();
    var eventWriter = new FakeAssessmentEventWriter();
    var handler = new UpdateInsightContentCommandHandler(assessmentRepo, insightRepo, eventWriter);

    var insightId = Guid.NewGuid();
    var result = await handler.Handle(
      new UpdateInsightContentCommand(assessmentId, insightId, "user content"),
      CancellationToken.None);

    Assert.NotNull(result);
    Assert.True(result!.CreatedNew);

    var stored = await insightRepo.GetByIdAsync(insightId);
    Assert.NotNull(stored);
    Assert.Equal("user content", stored!.Content);

    var @event = Assert.IsType<UserUpdatedInsightEvent>(eventWriter.LastAppendedEvent);
    Assert.Equal(assessmentId, @event.AssessmentId);
    Assert.Equal(insightId, @event.InsightId);
    Assert.Equal("user content", @event.Content);
  }

  [Fact]
  public async Task Handle_UpdatesInsight_WhenAlreadyExists()
  {
    var assessmentId = Guid.NewGuid();
    var assessmentRepo = new FakeAssessmentRepository();
    assessmentRepo.Seed(new Assessment { Id = assessmentId, CreatedBy = Guid.NewGuid() });

    var insightId = Guid.NewGuid();
    var insightRepo = new FakeInsightContentRepository();
    await insightRepo.AddAsync(new InsightContent
    {
      Id = insightId,
      AssessmentId = assessmentId,
      Content = "old",
      CreatedAt = DateTime.UtcNow,
      CreatedBy = Guid.Empty
    });

    var eventWriter = new FakeAssessmentEventWriter();
    var handler = new UpdateInsightContentCommandHandler(assessmentRepo, insightRepo, eventWriter);

    var result = await handler.Handle(
      new UpdateInsightContentCommand(assessmentId, insightId, "updated"),
      CancellationToken.None);

    Assert.NotNull(result);
    Assert.False(result!.CreatedNew);

    var stored = await insightRepo.GetByIdAsync(insightId);
    Assert.Equal("updated", stored!.Content);

    var @event = Assert.IsType<UserUpdatedInsightEvent>(eventWriter.LastAppendedEvent);
    Assert.Equal("updated", @event.Content);
  }

  private sealed class FakeAssessmentRepository : IAssessmentRepository
  {
    private readonly Dictionary<Guid, Assessment> _assessments = new();

    public void Seed(Assessment assessment) => _assessments[assessment.Id] = assessment;

    public Task AddAsync(Assessment assessment)
    {
      _assessments[assessment.Id] = assessment;
      return Task.CompletedTask;
    }

    public Task<Assessment?> GetByIdAsync(Guid id)
    {
      _assessments.TryGetValue(id, out var assessment);
      return Task.FromResult<Assessment?>(assessment);
    }

    public Task UpdateAsync(Assessment assessment)
    {
      _assessments[assessment.Id] = assessment;
      return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
      _assessments.Remove(id);
      return Task.CompletedTask;
    }
  }

  private sealed class FakeInsightContentRepository : IInsightContentRepository
  {
    private readonly Dictionary<Guid, InsightContent> _insights = new();

    public Task AddAsync(InsightContent insightContent)
    {
      _insights[insightContent.Id] = insightContent;
      return Task.CompletedTask;
    }

    public Task<InsightContent?> GetByIdAsync(Guid id)
    {
      _insights.TryGetValue(id, out var insight);
      return Task.FromResult(insight);
    }

    public Task UpdateAsync(InsightContent insightContent)
    {
      _insights[insightContent.Id] = insightContent;
      return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
      _insights.Remove(id);
      return Task.CompletedTask;
    }
  }

  private sealed class FakeAssessmentEventWriter : IAssessmentEventWriter
  {
    public object? LastAppendedEvent { get; private set; }

    public Task<Guid> CreateAsync(AssessmentCreated @event, CancellationToken cancellationToken)
      => throw new NotSupportedException();

    public Task AppendAsync<TEvent>(Guid assessmentId, TEvent @event, CancellationToken cancellationToken)
      where TEvent : class
    {
      LastAppendedEvent = @event;
      return Task.CompletedTask;
    }
  }
}
