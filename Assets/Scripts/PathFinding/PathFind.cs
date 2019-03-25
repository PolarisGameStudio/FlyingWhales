﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PathFind {
    public static class PathFind {
        public static Path<Node> FindPath<Node>(Node start, Node destination, Func<Node, Node, double> distance, Func<Node, double> estimate
            , PATHFINDING_MODE pathfindingMode, object data = null)
            where Node : HexTile, IHasNeighbours<Node> {

            var closed = new HashSet<Node>();
            var queue = new PriorityQueue<double, Path<Node>>();
            queue.Enqueue(0, new Path<Node>(start));
            Node lastStep = start;

            Region region1 = start.region;
            Region region2 = destination.region;

            while (!queue.IsEmpty) {
                var path = queue.Dequeue();
                if (closed.Contains(path.LastStep))
                    continue;
                if (path.LastStep.Equals(destination))
                    return path;

                closed.Add(path.LastStep);
                lastStep = path.LastStep;

                double d;
                Path<Node> newPath;
                if (pathfindingMode == PATHFINDING_MODE.REGION_CONNECTION) {
                    foreach (Node n in path.LastStep.AllNeighbours) {
                        if (n.region.id != region1.id && n.region.id != region2.id) {
                            //path cannot pass through other regions
                            continue;
                        }
                        if (n.isOuterTileOfRegion && n.id != start.id && n.id != destination.id && !start.AllNeighbours.Contains(n) && !destination.AllNeighbours.Contains(n)) {
                            continue; //skip tiles that are outer tiles of the region, that is not the start or the destination tile
                        }

                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else if (pathfindingMode == PATHFINDING_MODE.LANDMARK_CONNECTION) {
                    foreach (Node n in path.LastStep.LandmarkConnectionTiles) {
                        if (n.region.id != region1.id && n.region.id != region2.id) {
                            //path cannot pass through other regions
                            continue;
                        }
                        if (n.hasLandmark && n.id != start.id && n.id != destination.id) {
                            //current node has a landmark and is not the start or destination
                            //skip this node
                            continue;
                        }
                        if (n.AllNeighbourRoadTiles.Count > 0 && n.id != start.id && n.id != destination.id
                            && !start.AllNeighbours.Contains(n) && !destination.AllNeighbours.Contains(n)) {
                            //current node has adjacent roads, check if it is a neighbour of start or destination
                            //if it is, allow the path
                            //else skip this node
                            continue;
                        }

                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else if (pathfindingMode == PATHFINDING_MODE.LANDMARK_ROADS) {
                    foreach (Node n in path.LastStep.NoWaterTiles) {
                        if (n.region.id != region1.id && n.region.id != region2.id) {
                            //path cannot pass through other regions
                            continue;
                        }
                        if (n.hasLandmark && n.id != start.id && n.id != destination.id) {
                            continue;
                        }

                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else if (pathfindingMode == PATHFINDING_MODE.POINT_TO_POINT) {
                    foreach (Node n in path.LastStep.allNeighbourRoads) {
                        if (n.hasLandmark && n.id != start.id && n.id != destination.id) {
                            continue;
                        }
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else if (pathfindingMode == PATHFINDING_MODE.USE_ROADS) {
                    foreach (Node n in path.LastStep.allNeighbourRoads) {
                        //if (n.tileTag != start.tileTag) {
                        //    continue;
                        //}
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else if (pathfindingMode == PATHFINDING_MODE.UNRESTRICTED) {
                    foreach (Node n in path.LastStep.AllNeighbours) {
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else if (pathfindingMode == PATHFINDING_MODE.PASSABLE) {
                    foreach (Node n in path.LastStep.PassableNeighbours) {
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else if (pathfindingMode == PATHFINDING_MODE.PASSABLE_REGION_ONLY) {
                    foreach (Node n in path.LastStep.PassableNeighbours) {
                        if (data == null) {
                            throw new Exception("There is no provided data!");
                        } else if (data is Region) {
                            if (n.region.id != (data as Region).id) {
                                continue; //skip tiles that are not part of the region
                            }
                        }
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else if (pathfindingMode == PATHFINDING_MODE.REGION_ISLAND_CONNECTION) {
                    foreach (Node n in path.LastStep.AllNeighbours) {
                        if (data == null) {
                            throw new Exception("There is no provided data!");
                        } else if (data is Region) {
                            if (n.region.id != (data as Region).id) {
                                continue; //skip tiles that are not part of the region
                            }
                            if ((data as Region).outerTiles.Contains(n) && n.id != start.id && n.id != destination.id && !start.AllNeighbours.Contains(n) && !destination.AllNeighbours.Contains(n)) {
                                continue; //skip tiles that are outer tiles of the region, that is not the start or the destination tile
                            }
                        }
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else if (pathfindingMode == PATHFINDING_MODE.AREA_ONLY) {
                    foreach (Node n in path.LastStep.PassableNeighbours) {
                        if (!(data is Area)) {
                            throw new Exception("There is no provided data!");
                        } else {
                            Area area = data as Area;
                            if (n.areaOfTile == null || n.areaOfTile.id != area.id) {
                                continue; //skip
                            }
                        }
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else {
                    foreach (Node n in path.LastStep.ValidTiles) {
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                }
            }
            return null;
        }

        public static Path<Node> FindPath<Node>(Node start, Node destination, Func<Node, Node, double> distance, Func<Node, double> estimate,
            GRID_PATHFINDING_MODE pathMode, Func<Node, object[], List<Node>> tileGetFunction = null, params object[] args)
            where Node : LocationGridTile, IHasNeighbours<Node> {

            var closed = new HashSet<Node>();
            var queue = new PriorityQueue<double, Path<Node>>();
            queue.Enqueue(0, new Path<Node>(start));
            Node lastStep = start;

            while (!queue.IsEmpty) {
                var path = queue.Dequeue();
                if (closed.Contains(path.LastStep))
                    continue;
                if (path.LastStep.Equals(destination))
                    return path;

                closed.Add(path.LastStep);
                lastStep = path.LastStep;

                double d;
                Path<Node> newPath;
                if (tileGetFunction != null) {
                    List<Node> validTiles = tileGetFunction(path.LastStep, args);
                    foreach (Node n in validTiles) {
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else {
                    switch (pathMode) {
                        case GRID_PATHFINDING_MODE.NORMAL:
                            foreach (Node n in path.LastStep.ValidTiles) {
                                d = distance(path.LastStep, n);
                                newPath = path.AddStep(n, d);
                                queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                            }
                            break;
                        case GRID_PATHFINDING_MODE.ROADS_ONLY:
                            foreach (Node n in path.LastStep.RoadTiles) {
                                d = distance(path.LastStep, n);
                                newPath = path.AddStep(n, d);
                                queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                            }
                            break;
                        case GRID_PATHFINDING_MODE.REALISTIC:
                            foreach (Node n in path.LastStep.RealisticTiles) {
                                if (n.structure != null && n.structure.location.areaType != AREA_TYPE.DUNGEON) {
                                    if (n.tileType == LocationGridTile.Tile_Type.Structure && n.structure != start.structure && n.structure != destination.structure) {
                                        continue;
                                    }
                                }
                                d = distance(path.LastStep, n);
                                newPath = path.AddStep(n, d);
                                queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                            }
                            break;
                        default:
                            foreach (Node n in path.LastStep.ValidTiles) {
                                d = distance(path.LastStep, n);
                                newPath = path.AddStep(n, d);
                                queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                            }
                            break;
                    }
                }
                
                
            }
            return null;
        }
    }
}