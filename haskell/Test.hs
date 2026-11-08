#!/usr/bin/env stack
{- stack script --resolver lts-24.66 --system-ghc
  --package process,random
-}

import Data.List (sortOn)
import System.Random (randomIO)

main :: IO ()
main = putStrLn "Hello World"

shuffle :: [a] -> IO [a]
shuffle xs = do
  keys <- mapM (const randomIO) xs :: IO [Int]
  return . map snd . sortOn fst $ zip keys xs
