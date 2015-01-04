open System

type IntTree = 
    | Leaf of int
    | Node of IntTree * IntTree

let imbalancedTree =
    Node (Leaf 8, Node 
            (Node (Leaf 5,Leaf 3), Node(Leaf 7,Leaf 9)))

// Return value by calling the continuation
let rec sumTreeCont tree cont =
  match tree with
  | Leaf(n)    -> cont(n)
  | Node(left, right) -> 
      // Recursively sum left sub-tree
      sumTreeCont left (fun leftSum ->
        // Then recursively sum right sub-tree
        sumTreeCont right (fun rightSum ->
          // Finally, call the continuation with the result
          cont(leftSum + rightSum)))

let rec sumTree tree cont =
   match tree with
   | Leaf(n) -> cont n
   | Node(left, right) -> 
       sumTree left (fun leftSum ->
         sumTree right (fun rightSum -> 
           cont (leftSum + rightSum) ))

printfn "%d" (sumTree imbalancedTree ((*)2))