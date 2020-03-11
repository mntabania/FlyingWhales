using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inner_Maps;
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
                if (pathfindingMode == PATHFINDING_MODE.UNRESTRICTED) {
                    foreach (Node n in path.LastStep.AllNeighbours) {
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
                                 case GRID_PATHFINDING_MODE.UNCONSTRAINED:
                                     foreach (Node n in path.LastStep.FourNeighbours()) {
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
        
        public static Path<Node> FindPath<Node>(Node start, Node destination, Func<Node, Node, double> distance, Func<Node, double> estimate)
                     where Node : Region, IHasNeighbours<Node> {
         
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

                         foreach (Node n in path.LastStep.ValidTiles) {
                             var d = distance(path.LastStep, n);
                             var newPath = path.AddStep(n, d);
                             queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                         }
                     }
                     return null;
                 }
    }
}