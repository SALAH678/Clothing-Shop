using Application.Features.Dashboard.Dtos;
using MediatR;

namespace Application.Features.Dashboard.Query.GetOverview;

public sealed record GetOverviewQuery : IRequest<OverviewDto>;


