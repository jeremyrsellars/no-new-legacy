(ns sheepish.g-framework-assisted-sheepish-tests
  (:require [clojure.spec.alpha :as s]
            [clojure.spec.gen.alpha :as gen]
            [clojure.string :as string]
            [clojure.test :as test :refer [deftest testing is]]
            clojure.test.check.generators #_ "This is necessary at runtime"
            [clojure.spec.test.alpha :as stest]
            [clojure.test.check.properties :as prop]
            [clojure.test.check.clojure-test :refer [defspec]]
            [sheepish.core :refer [sheep-bleat?]]
            [sheepish.f-better-sheepish-examples :as f]))

(defspec sheep-bleat?_matches_oracle
  ; Run the test for each of generated examples
  (prop/for-all [text (s/gen ::f/sheepish-like-string 4)]
    (= (some? (re-find #"^baa+$" text))  ; true when a match is found, false when nil is returned.
       (sheep-bleat? text))))

; (clojure.test/run-tests)
; -or-
; (sheep-bleat?_matches_oracle)
; -or-
; (sheep-bleat?_matches_oracle 12 :seed 1549979449257)
